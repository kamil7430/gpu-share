FROM python:3.12

WORKDIR /app

COPY search/requirements.txt .

RUN pip install -r requirements.txt

COPY search search

WORKDIR /app/search

ENV PYTHONUNBUFFERED=1

CMD ["python", "server.py"]
