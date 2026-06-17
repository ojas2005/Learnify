import mysql.connector
import os

config = {
    'host': 'mysql-1651077c-tiwariojas578-c2c8.k.aivencloud.com',
    'port': 15572,
    'user': 'avnadmin',
    'password': '',
    'database': 'defaultdb',
    'ssl_disabled': False
}

try:
    conn = mysql.connector.connect(**config)
    cursor = conn.cursor()
    
    with open('init_db.sql', 'r') as f:
        sql = f.read()
    
    # Split by semicolon to run statements individually
    # Note: simple splitting by ; might fail with complex SQL, but our DDL is simple.
    statements = sql.split(';')
    for statement in statements:
        if statement.strip():
            print(f"Executing: {statement.strip()[:50]}...")
            cursor.execute(statement)
    
    conn.commit()
    print("Database schema initialized successfully.")
    
except Exception as e:
    print(f"Error: {e}")
finally:
    if 'conn' in locals() and conn.is_connected():
        cursor.close()
        conn.close()
