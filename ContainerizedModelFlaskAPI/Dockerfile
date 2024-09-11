# syntax=docker/dockerfile:1

FROM python:3.9.13

WORKDIR /code

COPY requirements.txt .

ENV PYTHONIOENCODING=utf8
ENV PYTHONLEGACYWINDOWSSTDIO=utf-8
RUN pip3 install -r requirements.txt

COPY . .

EXPOSE 50505

ENTRYPOINT ["gunicorn", "app:app"]