# GroupChatting

This project is a complete, production-ready backend for a real-time chat application, built with ASP.NET Core. It features a clean, scalable Onion Architecture, polyglot persistence using PostgreSQL and MongoDB, and full support for CI/CD and containerized deployment with Docker.

## Features

* **Authentication:**
    * JWT-based access and refresh token mechanism for secure, persistent sessions.
    * Endpoints for user registration and login.
* **Real-Time Messaging:**
    * WebSocket communication for instant message delivery using SignalR.
    * Users can send text messages and upload files to groups.
    * Users can edit and soft-delete their own messages.
* **Group Management:**
    * Create and list public and private groups.
    * Full invite/apply/approve system for managing private group membership.
* **Data Retrieval:**
    * Paginated message history for efficient loading.
    * Full-text search functionality for messages within a specific group.
* **DevOps & Deployment:**
    * Fully containerized with Docker and Docker Compose for easy local setup.
    * CI/CD pipeline using GitHub Actions for automated building, linting, and testing.
    * Automated Docker image publishing to a container registry.
    * Includes a full suite of unit and integration tests using xUnit.

## Tech Stack

* **Backend:** ASP.NET Core 8, C#
* **Databases:**
    * PostgreSQL (for relational data like users and groups)
    * MongoDB (for message storage and search)
* **Real-time Communication:** SignalR
* **Architecture:** Onion Architecture, Polyglot Persistence
* **Testing:** xUnit
* **Containerization:** Docker
* **CI/CD:** GitHub Actions
* **Deployment:** Heroku

---

##  Setup and Running Locally

To run this project on your local machine, you only need Docker Desktop installed and running while you build and run the application.

### Prerequisites

* [Git](https://git-scm.com/downloads)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/)
* [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0) 

### Configuration

1.  **Clone the repository:**
    ```bash
    git clone <your-repo-url>
    cd <your-repo-name>
    ```
2.  **Configure Secrets:**
    * In the root of the project, you will find a file named `.env.example`.
    * Create a copy of this file and name it `.env`.
    * Open the new `.env` file and provide a long, random string for the `JWT_SECRET` value.

### Run the Application

Once configured, run the following command from the project's root directory:
```bash
docker-compose up
```

The application and its databases will start.

Accessing the API
The API will be available at http://localhost:8080. You can access the Swagger UI for documentation and testing at:

http://localhost:8080/swagger

---

##  API Usage

The API is fully documented using Swagger. Once the application is running locally, you can access the interactive documentation to explore and test all endpoints at:

**`http://localhost:8080/swagger`**

---

##  CI/CD Overview

This project uses a Continuous Integration/Continuous Delivery (CI/CD) pipeline configured with GitHub Actions. The workflow is defined in the `.github/workflows/dotnet.yml` file.

The pipeline is triggered on every push or pull request to the `main` branch and performs the following steps:
1.  **Build:** Compiles the .NET solution to ensure there are no compilation errors.
2.  **Lint:** Uses a code formatter to check for consistent styling.
3.  **Test:** Runs the complete suite of xUnit tests.
4.  **Publish:** If all previous steps succeed, it builds the production Docker image and pushes it to the GitHub Container Registry (GHCR), making it available for deployment.

---

##  Deployment

This application is configured for deployment to **Heroku** using the .NET Buildpack and a `Procfile` for release phase migrations. I have chosen Heroku for its simplicity and free plan for students, making it easy to deploy and manage and affordable for small projects.

### Prerequisites

* A [Heroku Account](https://signup.heroku.com/)
* The [Heroku CLI](https://devcenter.heroku.com/articles/heroku-cli)
* A [MongoDB Atlas](https://www.mongodb.com/cloud/atlas) account for MongoDB setup

### Deployment Steps

1.  **Log in to the Heroku CLI:**
    ```bash
    heroku login
    ```
2.  **Create the Heroku App:**
    ```bash
    heroku create groupchatting
    ```
3.  **Provision Add-ons:**
    * **PostgreSQL:**
        ```bash
        heroku addons:create heroku-postgresql:essential-0 -a groupchatting
        ```
    * **MongoDB:** Create a free cluster on the [MongoDB Atlas](https://www.mongodb.com/cloud/atlas) website, configure network access to `0.0.0.0/0`, and get the connection string.

4.  **Set Environment Variables (Config Vars):**
    ```bash
    # Set the JWT Secret
    heroku config:set AppSettings__Token="your_super_long_and_random_secret_key" -a groupchatting

    # Set the MongoDB Connection String from Atlas
    # Connection string will be created in the Atlas UI, replace <username> and <password> with your credentials
    heroku config:set MONGODB_URI="mongodb+srv://..." -a groupchatting
    ```
5.  **Deploy:** 
    Add Heroku as a remote for your Git repository and push.
    You can configure GitHub integration in the Deploy tab of apps in the Heroku Dashboard. For more information refer to the [Heroku documentation](https://devcenter.heroku.com/articles/github-integration).
    After setting up the GitHub integration, you can push your code to the `main` branch, and Heroku will automatically build and deploy your application.

    ```bash
    git push heroku main
    ```

6.  Click on "Open App" button in the Heroku Dashboard to access your deployed application. Add /swagger to the URL to access the Swagger UI for API documentation and testing.

    ```bash
    https://groupchatting-.....herokuapp.com/swagger/index.html
    ```

---

## Assumptions and Tradeoffs

This section outlines some of the key design decisions made during the development of this project.

* **Database Choice (MongoDB vs. Redis):** MongoDB was chosen for message storage primarily for its powerful querying capabilities and built-in support for full-text search, which was a core requirement. While Redis may offer faster raw read/write speeds, implementing a robust search feature would have required additional complexity or tools.

* **File Storage (Local Disk vs. Cloud Storage):** User-uploaded files are saved to the local filesystem of the server. This approach was chosen for its simplicity and to avoid dependencies on external cloud services for this assignment. In a scalable, multi-server production environment, a distributed object storage service like Amazon S3 or Azure Blob Storage would be required to ensure files are accessible from all instances and are not lost if a server is terminated.

* **Architecture (Onion Architecture):** The Onion Architecture was implemented to create a clean separation of concerns, making the application highly testable and maintainable. It ensures that the core business logic is independent of external frameworks like databases or the web API itself.

* **Deployment (Heroku Buildpack vs. Docker):** The final deployment was configured using Heroku's native .NET Buildpack and a `Procfile` for its robust and well-integrated release phase management for database migrations. The application is fully containerized with Docker for local development and CI/CD

---
##  Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue if you find any bugs or have suggestions for improvements.

