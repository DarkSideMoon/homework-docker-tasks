# instruction initializes a new build stage and sets the Base Image for subsequent instructions
FROM node:17 as build

# instruction sets the working directory
WORKDIR /usr/src/app

# instruction copies new files or directories from <src> and adds them to the filesystem of the container at the path <dest>
COPY package*.json ./

RUN npm info
# instruction will execute any commands in a new layer on top of the current image and commit the results. 
# The resulting committed image will be used for the next step in the Dockerfile.
RUN npm install

# instruction copies new files or directories from <src> and adds them to the filesystem of the container at the path <dest>
COPY . .

# instruction informs Docker that the container listens on the specified network ports
EXPOSE 3000

# There can only be one CMD instruction in a Dockerfile
# The main purpose of a CMD is to provide defaults for an executing container
CMD ["npm", "start"]