# Library Management System

The **Library Management System** is an efficient and user-friendly software solution designed to streamline and automate the management of a library's operations. This system simplifies tasks such as managing books, authors, categories, subscribers, and user roles, as well as tracking book rentals. The application provides a detailed dashboard with important statistics, allows seamless search for books, authors, and categories, and generates comprehensive reports on rentals, including delayed rentals. It aims to reduce manual efforts, improve library organization, and enhance user experience.

This system supports library administrators in efficiently overseeing library operations and helps users manage their book rentals and subscriptions effectively.

## Features

### **Dashboard**
The **Dashboard** serves as the central hub of the system. It offers a quick overview of various key metrics such as:
- Total number of books in the library
- Total active subscribers
- Number of books currently rented out
- Pending rental returns
- Overall statistics on overdue rentals
This gives administrators and staff a bird's-eye view of the library’s current state and activities.

### **Categories**
The **Categories** module allows administrators to classify books into various categories, making it easier for users to browse and search for books based on their interests. Categories might include genres such as Fiction, Non-fiction, Science, Technology, History, etc.

### **Authors**
The **Authors** section helps the system maintain records of authors and their published books. Each author’s details, including their name, biography, and books they have written, are kept organized and accessible for reference.

### **Books**
The **Books** module is at the heart of the library management system. This feature allows administrators to:
- Add new books to the library database
- Update information about existing books
- View the availability status of books (available or rented)
- Maintain records of each book’s category, author, and rental history

### **Subscribers**
The **Subscribers** module is responsible for managing library members who borrow books. Administrators can add new subscribers, update their details, and view their borrowing history. This helps in keeping track of active members, overdue rentals, and subscriptions.

### **Users**
The **Users** module is for managing the access control to the system. Admins can create and assign different roles (like Administrator, Staff, User) with varying levels of access and permissions. This ensures that only authorized users have access to sensitive functionalities, such as adding or deleting books or managing rental data.

### **Search**
The **Search** functionality enables users and administrators to easily find books, authors, or categories based on various criteria. It supports flexible search queries to quickly locate a specific book or author, improving the overall user experience.

### **Reports**
Reports provide comprehensive insights into library operations and rentals, and include:
- **Books Report**: Detailed listing of all books available in the library, with their status (available or rented), author, and category.
- **Rentals Report**: This report tracks all rental activities, including current rentals, rental history, and due dates.
- **Delayed Rentals Report**: Identifies overdue rentals, helping administrators follow up with users who have failed to return books on time.

## Tech Stack

- **Backend**: The backend of the system is built using [ASP.NET Core](https://dotnet.microsoft.com/en-us/learn/aspnet), a high-performance and cross-platform framework that ensures scalability and reliability.
- **Database**: The system utilizes [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server) as the database for storing and managing all library-related data, ensuring data integrity and quick access.
- **Frontend**: The frontend uses standard web technologies including [HTML](https://developer.mozilla.org/en-US/docs/Web/HTML), [CSS](https://developer.mozilla.org/en-US/docs/Web/CSS), [JavaScript](https://developer.mozilla.org/en-US/docs/Web/JavaScript), and [Bootstrap](https://getbootstrap.com/), a popular front-end framework for building responsive and mobile-first websites. Bootstrap ensures that the system's user interface is both attractive and responsive, adapting seamlessly to various screen sizes and devices.

## Setup

Follow the instructions below to set up the Library Management System on your local machine or server.

1. **Clone the repository**:
   To begin, clone the repository to your local machine using Git:
   ```bash
   git clone https://github.com/AbdelrahmanSaber74/Bookify.git
