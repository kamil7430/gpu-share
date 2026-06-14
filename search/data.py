from sqlalchemy import create_engine, text
from urllib.parse import quote_plus
import os

db_user = os.getenv("POSTGRES_USER")
db_password = os.getenv("POSTGRES_PASSWORD")
db_password = quote_plus(db_password)
db_host = os.getenv("POSTGRES_DB_HOST", "db")
db_port = os.getenv("POSTGRES_DB_PORT")
db_name = os.getenv("POSTGRES_DB")

DB_URL = f"postgresql+psycopg2://{db_user}:{db_password}@{db_host}:{db_port}/{db_name}"
engine = create_engine(
    DB_URL,
    pool_size=20,
    max_overflow=40,
    pool_timeout=10,
)


def get_devices():
    with engine.connect() as conn:
        result = conn.execution_options(stream_results=True).execute(
            text("SELECT * FROM devices")
        )

        columns = result.keys()
        return [dict(zip(columns, row)) for row in result]
