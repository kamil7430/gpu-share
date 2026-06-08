from sqlalchemy import create_engine, text
from urllib.parse import quote_plus
import os

db_user = os.getenv("POSTGRES_USER")
db_password = os.getenv("POSTGRES_PASSWORD")
db_password = quote_plus(db_password)
db_host = os.getenv("POSTGRES_DB_HOST", "db")
db_port = os.getenv("POSTGRES_DB_PORT")
db_name = os.getenv("POSTGRES_DB")

engine = create_engine(
    f"postgresql+psycopg2://{db_user}:{db_password}@{db_host}:{db_port}/{db_name}"
)


def get_devices():
    with engine.connect() as conn:
        result = conn.execute(text("SELECT * FROM devices"))

        columns = result.keys()
        return [dict(zip(columns, row)) for row in result.fetchall()]


gpu_data = get_devices()
