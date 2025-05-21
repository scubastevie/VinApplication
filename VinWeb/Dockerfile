# Dockerfile
FROM node:18-alpine

WORKDIR /app

# Copy only package files first (for better Docker caching)
COPY package*.json ./

# Install dependencies (this is where axios is pulled in)
RUN npm install

# Copy the rest of your source code
COPY . .

# Start Vite
CMD ["npm", "run", "dev"]
