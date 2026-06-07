from sqlalchemy import create_engine, text


engine = create_engine(
    "postgresql+psycopg2://postgres:password@db:5432/postgres"
)

def get_devices():
    with engine.connect() as conn:
        result = conn.execute(text("SELECT * FROM devices"))

        columns = result.keys()
        return [dict(zip(columns, row)) for row in result.fetchall()]
