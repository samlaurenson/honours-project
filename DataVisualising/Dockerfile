# syntax=docker/dockerfile:1
FROM python:3.8-slim-buster
EXPOSE 5000

WORKDIR /service

COPY requirements.txt requirements.txt
RUN pip3 install -r requirements.txt
COPY . .

ENV FLASK_APP=graphing.py

CMD [ "python", "-m" , "flask", "run", "--host=0.0.0.0"]
