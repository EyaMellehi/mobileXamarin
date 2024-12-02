const express = require('express');
const { Client } = require('pg');

//const mysql = require('mysql2');
const bodyParser = require('body-parser');
const e = require('express');

const app = express();
app.use(express.json());
app.use(bodyParser.json());

const connectionString = 'postgresql://neondb_owner:R8IhSm2dFAKG@ep-nameless-mountain-a5lkhw43.us-east-2.aws.neon.tech/neondb?sslmode=require';

const client = new Client({
    connectionString: connectionString,  // Use the connection string directly
});

// Connect to the PostgreSQL database
client.connect((err) => {
    if (err) {
        console.error('Error connecting to PostgreSQL:', err.stack);
    } else {
        console.log('Connected to PostgreSQL');
    }
});
app.get('/api/status', (req, res) => {
    res.send('Server is up and running'); // You can customize this message as needed
});
// Endpoint for login
app.post('/api/login', (req, res) => {
    const { email, pwd } = req.body;
    console.log("Received email:", email);
    console.log("Received pwd:", pwd);

    // PostgreSQL uses $1, $2 for parameterized queries
    const query = "SELECT * FROM patient WHERE TRIM(email)=$1 AND TRIM(pwd) = $2";

    console.log(query);

    client.query(query, [email, pwd], (err, results) => {
        if (err) {
            console.error('Error during query execution:', err);
            return res.status(500).send('Erreur lors de l\'authentification');
        }
        console.log("Query Results:", results.rows);

        if (results.rows.length > 0) {
            res.json(results.rows[0]); // Return patient data on successful login
        } else {
            res.status(401).send('Email ou mot de passe incorrect');
        }
    });
});


// Configuration de la connexion à la base de données MySQL
/*const db = mysql.createConnection({
    host:'127.0.0.1',
    user:'root',
    password:'',
    database:'medicalsolution',
    port: 3308
});

// Connecter à la base de données
db.connect((err) => {
    if (err) {
        console.error('Erreur de connexion à la base de données:', err);
        return;
    }
    console.log('Connecté à la base de données MySQL');
});*/


app.get('/api/status', (req, res) => {
    res.send('Server is up and running'); // You can customize this message as needed
});
// Endpoint for login
app.post('/api/login', (req, res) => {
    const { email, pwd } = req.body;
    const query = "SELECT * FROM patient WHERE email = ? AND pwd = ?";
    client.query(query, [email, pwd], (err, results) => {
        if (err) {
            return res.status(500).send('Erreur lors de l\'authentification');
        }
        if (results.length > 0) {
            res.json(results[0]); // Return patient data on successful login
        } else {
            res.status(401).send('Email ou mot de passe incorrect');
        }
    });
});


// Endpoint to update specific fields of an appointment
app.put('/api/appointments/:id', (req, res) => {
    const appointmentId = req.params.id; // Appointment ID from the URL
    const { etat, date, hour } = req.body; // Fields to update

    // PostgreSQL query with placeholders
    const query = `
        UPDATE appointment 
        SET 
            etat = $1, 
            date = $2, 
            hour = $3 
        WHERE id = $4
        RETURNING *;  -- This returns the updated appointment, useful for debugging
    `;

    // Parameters for the query
    const params = [etat, date, hour, appointmentId];

    // Execute the query
    client.query(query, params, (err, result) => {
        if (err) {
            console.error('Error updating appointment:', err);
            return res.status(500).json({ message: 'Error updating appointment' });
        }

        if (result.rows.length > 0) {
            res.json({ message: 'Appointment updated successfully' });
        } else {
            res.status(404).json({ message: 'Appointment not found' });
        }
    });
});

//searching availability
app.get('/api/doctors/availability/:id', (req, res) => {
    const doctorId = req.params.id;

    const query = `
        SELECT
            id,
            date_a::text AS date_a,  
            to_char(hour_a, 'HH24:MI') AS hour_a
        FROM doctor_avail 
        WHERE doctor_id = $1;
        `;

    client.query(query, [doctorId], (err, results) => {
        if (err) {
            console.error('Error fetching availability:', err);
            return res.status(500).json({ message: 'Error fetching availability' });
        }
        res.json(results.rows);
    });
});

// PUT request pour déplacer un rendez-vous
app.put('/api/appointments/decal/:appointmentId', (req, res) => {
    const appointmentId = req.params.appointmentId;
    const { newDate, newHour, doctorId } = req.body;

    // 1. Mettre à jour la date et l'heure dans la table appointment
    const updateAppointmentQuery = `
        UPDATE appointment 
        SET date = $1, hour = $2, etat = 'encours'
        WHERE id = $3 RETURNING *;
    `;

    client.query(updateAppointmentQuery, [newDate, newHour, appointmentId], (err, result) => {
        if (err) {
            return res.status(500).json({ message: 'Error updating appointment', error: err.message });
        }

        // 2. Ajouter le créneau dans doctor_avail
        const insertAvailQuery = `
            INSERT INTO doctor_avail (doctor_id, date_a, hour_a)
            VALUES ($1, $2, $3) RETURNING *;
        `;

        client.query(insertAvailQuery, [doctorId, newDate, newHour], (err, result) => {
            if (err) {
                return res.status(500).json({ message: 'Error adding to doctor availability', error: err.message });
            }

            // Renvoie une réponse réussie
            res.status(200).json({ message: 'Appointment updated and availability added', data: result.rows });
        });
    });
});








// Endpoint to delete a doctor availability by ID
app.delete('/api/doctor_avail/:id', (req, res) => {
    const availabilityId = req.params.id;

    const query = 'DELETE FROM doctor_avail WHERE id = $1';

    client.query(query, [availabilityId], (err, result) => {
        if (err) {
            console.error('Error deleting doctor availability:', err);
            return res.status(500).json({ message: 'Error deleting doctor availability' });
        }

        if (result.rowCount > 0) {
            res.json({ message: 'Doctor availability deleted successfully' });
        } else {
            res.status(404).json({ message: 'Doctor availability not found' });
        }
    });
});



//save appointment
app.post('/api/appointments', async (req, res) => {
    const { codem, patient, date, etat, hour } = req.body;

    // Validate input fields
    if (!codem || !patient || !date || !etat || !hour) {
        return res.status(400).json({ error: 'All fields are required' });
    }

    const sql = `INSERT INTO appointment (codem, patient, date, etat, hour) VALUES ($1, $2, $3, $4, $5) RETURNING id`;

    try {
        // Execute the query
        const result = await client.query(sql, [codem, patient, date, etat, hour]);

        // Return success response with the created appointment ID
        res.status(201).json({ message: 'Appointment created successfully', id: result.rows[0].id });
    } catch (err) {
        // Log the error for debugging
        console.error('Error occurred while creating the appointment:', err);

        // Handle specific PostgreSQL errors if needed
        if (err.code === '23505') {
            res.status(400).json({ error: 'Duplicate entry. The appointment already exists.' });
        } else if (err.code === '23503') {
            res.status(400).json({ error: 'Foreign key constraint fails. Invalid patient or doctor ID.' });
        } else {
            // Generic error response for other cases
            res.status(500).json({ error: 'An error occurred while creating the appointment' });
        }
    }
});



// CRUD : Create -> Ajouter un patient
app.post('/api/patient', (req, res) => {
    console.log('Incoming Data:', req.body);  
    const { nom, telephone, email, pwd } = req.body;
    const query = "INSERT INTO patient (nom, telephone, email, pwd) VALUES ($1, $2, $3, $4)"; 
    client.query(query, [nom, telephone, email, pwd], (err, result) => {
        if (err) {
            console.error(err);
            res.status(500).send("Erreur lors de l'ajout de patient");
        } else {
            res.status(201).send('Patient ajouté avec succès');
        }
    });
});





// Endpoint to get all appointments for a specific patient
app.get('/api/appointments/patient/:patientId', (req, res) => {
    console.log('Request Params:', req.params);
    const patientId = req.params.patientId;  // Get patientId from the request parameters

    const query = `
        SELECT
            a.id,
            a.codem,
            TO_CHAR(a.date, 'YYYY-MM-DD') AS date,  -- Use TO_CHAR for date formatting in PostgreSQL
            a.hour,
            a.etat,
            d.nom AS doctor_name, 
            d.specialite AS doctor_specialty
        FROM 
            appointment a
        JOIN 
            doctor d ON a.codem = d.codem
        WHERE 
            a.patient = $1;
    `;

    client.query(query, [patientId], (err, results) => {
        if (err) {
            console.error('Error fetching appointments:', err);
            return res.status(500).json({ message: 'Error fetching appointments' });
        }

        // If no appointments found for the patient
        if (results.rows.length === 0) {
            console.log('No appointments found for patient:', patientId);
            return res.status(204).json({ message: 'You don’t have any appointments' });  // No appointments for this patient
        }

        // Filter active appointments (encours or confirme)
        const activeAppointments = results.rows.filter(appointment => appointment.etat === 'encours' || appointment.etat === 'confirme');

        if (activeAppointments.length > 0) {
            console.log('Active appointments:', activeAppointments);
            return res.json(activeAppointments);  // Return active appointments
        } else {
            // If no active appointments but patient exists
            console.log('Patient has no active appointments, possibly all cancelled.');
            return res.status(204).json({ message: 'You don’t have any appointments' });  // No content (only cancelled appointments)
        }
    });
});

/*app.get('/api/appointments/patient/:patientId', (req, res) => {
    console.log('Request Params:', req.params);
    console.log('Request Body:', req.body); // Should be undefined for GET
    console.log('Request Query:', req.query);
    const patientId = req.params.patientId;  // Get patientId from the request parameters

    const query = `
        SELECT
            a.id,
            a.codem,
            TO_CHAR(a.date, 'YYYY-MM-DD') AS date,  -- Use TO_CHAR for date formatting in PostgreSQL
            a.hour,
            a.etat,
            d.nom AS doctor_name, 
            d.specialite AS doctor_specialty
        FROM 
            appointment a
        JOIN 
            doctor d ON a.codem = d.codem
        WHERE 
            a.patient = $1 AND (a.etat = 'encours' OR a.etat = 'confirme');
    `;

    client.query(query, [patientId], (err, results) => {
        if (err) {
            console.error('Error fetching appointments:', err);
            return res.status(500).json({ message: 'Error fetching appointments' });
        }

        if (results.rows.length > 0) {
            console.log('Results:', results.rows);
            res.json(results.rows);  // Return appointments
        } else {
            console.log('No appointments found for patient:', patientId);
            res.status(204).json();  // No content found (status 204)
        }
    });
});*/






//cancel appointment
app.put('/api/appointments/cancel/:appointmentId', (req, res) => {
    console.log('Request Params:', req.params);

    const appointmentId = req.params.appointmentId;

    if (!appointmentId || isNaN(appointmentId)) {
        return res.status(400).json({ message: 'Invalid appointment ID' });
    }

    // Start a transaction to ensure both operations are executed atomically
    client.query('BEGIN', (err) => {
        if (err) {
            console.error('Error starting transaction:', err.message);
            return res.status(500).json({ message: 'Error starting transaction', error: err.message });
        }

        // Step 1: Update appointment status to 'annule'
        const updateQuery = `
            UPDATE appointment
            SET etat = 'annule'
            WHERE id = $1
            RETURNING codem AS doctor_id, date AS date_a, hour AS hour_a;
        `;

        client.query(updateQuery, [appointmentId], (err, result) => {
            if (err) {
                console.error('Error updating appointment:', err.message);
                return client.query('ROLLBACK', () => {
                    res.status(500).json({ message: 'Error updating appointment', error: err.message });
                });
            }

            if (result.rows.length > 0) {
                const { doctor_id, date_a, hour_a } = result.rows[0];

                // Step 2: Insert doctor availability back into the doctor_avail table
                const insertQuery = `
                    INSERT INTO doctor_avail (doctor_id, date_a, hour_a)
                    VALUES ($1, $2, $3);
                `;

                client.query(insertQuery, [doctor_id, date_a, hour_a], (err, insertResult) => {
                    if (err) {
                        console.error('Error inserting doctor availability:', err.message);
                        return client.query('ROLLBACK', () => {
                            res.status(500).json({ message: 'Error inserting doctor availability', error: err.message });
                        });
                    }

                    // Commit the transaction if everything went fine
                    client.query('COMMIT', (err) => {
                        if (err) {
                            console.error('Error committing transaction:', err.message);
                            return res.status(500).json({ message: 'Error committing transaction', error: err.message });
                        }

                        console.log(`Appointment with ID ${appointmentId} canceled and doctor availability restored`);
                        res.json({ message: 'Appointment canceled successfully', data: result.rows[0] });
                    });
                });
            } else { 
                console.log(`No appointment found with ID ${appointmentId}`);
                return res.status(404).json({ message: 'Appointment not found' });
            }
        });
    });
});





//select doctors
app.get('/api/doctors', (req, res) => {
    const query = "SELECT * FROM doctor ORDER BY specialite";
    client.query(query, (err, results) => {
        if (err) {
            return res.status(500).send("Erreur lors de la récupération des médecins");
        }
        res.json(results.rows); // Renvoie uniquement les données
    });
});




app.put('/api/appointments/update/:id', (req, res) => {
    const appointmentId = req.params.id;
    const { etat, date, hour, doctorId } = req.body; // Fields to update

    console.log('Updating appointment:', { appointmentId, etat, date, hour, doctorId });

    // Step 1: Get the current (old) date and hour for the appointment
    const getOldAppointmentQuery = `
        SELECT date, hour FROM appointment WHERE id = $1;
    `;
    client.query(getOldAppointmentQuery, [appointmentId], (err, result) => {
        if (err) {
            console.error('Error fetching old appointment:', err);
            return res.status(500).json({ message: 'Error fetching old appointment', error: err });
        }

        if (result.rows.length === 0) {
            console.error('Appointment not found');
            return res.status(404).json({ message: 'Appointment not found' });
        }

        const oldDate = result.rows[0].date;
        const oldHour = result.rows[0].hour;
        console.log('Old appointment:', { oldDate, oldHour });

        // Step 2: Delete the selected (new) slot from doctor_avail table
        const deleteSelectedAvailQuery = `
            DELETE FROM doctor_avail WHERE doctor_id = $1 AND date_a = $2 AND hour_a = $3;
        `;
        client.query(deleteSelectedAvailQuery, [doctorId, date, hour], (err) => {
            if (err) {
                console.error('Error deleting new availability slot:', err);
                return res.status(500).json({ message: 'Error deleting new availability slot', error: err });
            }

            // Step 3: Reinsert the old availability slot back into doctor_avail
            const insertOldAvailQuery = `
                INSERT INTO doctor_avail (doctor_id, date_a, hour_a)
                VALUES ($1, $2, $3);
            `;
            client.query(insertOldAvailQuery, [doctorId, oldDate, oldHour], (err) => {
                if (err) {
                    console.error('Error re-adding old availability:', err);
                    return res.status(500).json({ message: 'Error re-adding old availability', error: err });
                }

                // Step 4: Update the appointment with the new date and hour
                const updateAppointmentQuery = `
                    UPDATE appointment 
                    SET etat = $1, date = $2, hour = $3 
                    WHERE id = $4
                    RETURNING *;
                `;
                const updateParams = [etat, date, hour, appointmentId];

                client.query(updateAppointmentQuery, updateParams, (err, updateResult) => {
                    if (err) {
                        console.error('Error updating appointment:', err);
                        return res.status(500).json({ message: 'Error updating appointment', error: err });
                    }

                    if (updateResult.rows.length > 0) {
                        console.log('Appointment updated successfully');
                        res.json({ message: 'Appointment updated and availability updated successfully' });
                    } else {
                        console.error('Appointment not found after update');
                        res.status(404).json({ message: 'Appointment not found' });
                    }
                });
            });
        });
    });
});






app.listen(4003, () => {
    console.log('Serveur démarré sur http://192.168.1.6:4003');
});

// Endpoint to get available slots for a specific doctor
/*app.get('/api/doctor_avail/:doctorId', async (req, res) => {
    const doctorId = req.params.doctorId;

    const query = `
        SELECT id, doctor_id, date_a, hour_a
        FROM doctor_avail
        WHERE doctor_id = $1
    `;

    try {
        const result = await client.query(query, [doctorId]); // Use parameterized queries to prevent SQL injection
        res.json(result.rows); // Return the rows as JSON
    } catch (err) {
        console.error('Error fetching available slots:', err);
        res.status(500).json({ message: 'Error fetching available slots' });
    }
});*/




/*app.post('/api/doctor', (req, res) => {
    const { nom,specialite, cin, telephone, email, pwd } = req.body;

    console.log('Doctor data:', nom, specialite, cin, telephone, email, pwd);

    const query = "INSERT INTO doctor (nom, specialite, cin, telephone, email, pwd) VALUES (?, ?, ?, ?, ?, ?)";

    client.query(query, [nom, specialite, cin, telephone, email, pwd], (err, result) => {
        if (err) {
            console.error(err);
            return res.status(500).send("Erreur lors de l'ajout du médecin");
        }
        res.status(201).send('Médecin ajouté avec succès');
    });
});
app.get('/api/doctors', (req, res) => {
    const query = "SELECT * FROM doctor";
    client.query(query, (err, results) => {
        if (err) {
            return res.status(500).send("Erreur lors de la récupération des médecins");
        }
        res.json(results.rows); // Renvoie uniquement les données
    });
});*/