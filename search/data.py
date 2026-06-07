from sqlalchemy import create_engine, text


engine = create_engine(
    "postgresql+psycopg2://postgres:zaq1%40WSX@db:5432/gpu"
)

def get_devices():
    with engine.connect() as conn:
        result = conn.execute(text("SELECT * FROM devices"))

        columns = result.keys()
        return [dict(zip(columns, row)) for row in result.fetchall()]


gpu_data = get_devices()
