# All operating system dependencies, Cypress, and some browsers installed globally.	
FROM cypress/included:9.1.1

WORKDIR /app

COPY ./cypress ./cypress
COPY ./cypress.json ./cypress.json

RUN npx cypress run