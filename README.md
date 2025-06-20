# 🛒 PBL3 - SmartBuy Mobile

## 📋 Overview

SmartBuy Mobile is an e-commerce platform designed for mobile devices that allows users to browse, purchase, and review products. The platform includes features such as user authentication, product management, shopping cart functionality, order processing, and an admin dashboard for business analytics and product management.

## 🏗️ Project Structure

The project follows a modern architecture with separated backend and frontend:

-   **Backend**: ASP.NET Core Web API
-   **Frontend**: Vue.js
    -   Customer interface (Web application)
    -   Admin dashboard (Web application)

## ✨ Features

### 👤 Customer Features

-   User registration and authentication
-   Product browsing and searching
-   Product filtering by brands, categories, and other attributes
-   Shopping cart functionality
-   Order placement and tracking
-   Product reviews and ratings
-   Discount code application
-   Chatbot assistance

### 👨‍💼 Admin Features

-   Admin authentication and authorization
-   Dashboard with business analytics
-   Product management
-   Order management
-   Discount management
-   User management
-   Brand management

## 🔧 Technology Stack

### 🖥️ Backend

-   **Framework**: ASP.NET Core
-   **Database**: MySQL, Entity Framework Core
-   **Authentication**: JWT Tokens
-   **API Documentation**: Swagger/OpenAPI

### 📱 Frontend

-   **Framework**: Modern JavaScript framework (Vite-based)
-   **State Management**: (To be filled based on actual implementation)
-   **UI Components**: (To be filled based on actual implementation)

## 🚀 Getting Started

### 📋 Prerequisites

-   [.NET SDK](https://dotnet.microsoft.com/download) (version 8.0 or later)
-   [Node.js](https://nodejs.org/) (version 20.0 or later)
-   [npm](https://www.npmjs.com/) or [yarn](https://yarnpkg.com/)
-   [MySQL](https://www.mysql.com/) (version 8.0 or later)

### ⚙️ Backend Setup

1. Navigate to the backend API directory:

    ```
    cd backend/api
    ```

2. Copy and configure the app settings:

    ```
    cp appsettings.json.example appsettings.json
    ```

    - Update the database connection string
    - Configure JWT settings
    - Set up other required configurations

3. Update your database:

    Install EF Core CLI

    ```
    dotnet tool install --global dotnet-ef
    ```

    Create migration and update database

    ```
    dotnet ef migrations add Init && dotnet ef database update
    ```

4. Run the application:
    ```
    dotnet run
    ```
    The API will be available at `http://localhost:5000` by default.

### 🎨 Frontend Setup

#### 🛍️ Customer Interface

1. Navigate to the customer frontend directory:

    ```
    cd frontend/customer
    ```

2. Create and configure environment variables:

    ```
    cp .env.example .env
    ```

    Update the `.env` file with your configuration:

    ```
    VITE_GOOGLE_CLIENT_ID=YOUR_GOOGLE_CLIENT_ID
    VITE_API_URL=YOUR_API_URL
    VITE_PAYPAL_CLIENT_ID=YOUR_PAYPAL_CLIENT_ID
    ```

3. Install dependencies:

    ```
    npm install
    ```

4. Run the development server:
    ```
    npm run dev
    ```
    The customer interface will be available at `http://localhost:3000` by default.

#### 📊 Admin Interface

1. Navigate to the admin frontend directory:

    ```
    cd frontend/admin
    ```

2. Create and configure environment variables:

    ```
    cp .env.example .env
    ```

    Update the `.env` file with your configuration:

    ```
    VITE_API_URL=YOUR_API_URL
    ```

3. Install dependencies:

    ```
    npm install
    ```

4. Run the development server:
    ```
    npm run dev
    ```
    The admin interface will be available at `http://localhost:4000` by default.

## 📚 API Documentation

API documentation is available through Swagger UI at `http://localhost:5000/swagger` when the backend is running.

## 📄 License

This project is licensed under the [MIT License](LICENSE).

## 🤝 Contributors

-   Nguyễn Thị Thu Phương
-   Đặng Đăng Khoa
-   Nguyễn Quốc Nguyên
