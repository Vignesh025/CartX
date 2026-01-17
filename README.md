# CartX - E-Commerce Application

CartX is a full-featured e-commerce web application built with ASP.NET Core MVC, designed for managing products, orders, and user accounts with role-based access control.

## Table of Contents

- [Technologies Used](#technologies-used)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Architecture & Separation of Concerns](#architecture--separation-of-concerns)
- [Detailed Component Documentation](#detailed-component-documentation)
- [Getting Started](#getting-started)

---

## Technologies Used

### Core Framework
- **ASP.NET Core 10.0** - Modern, cross-platform web framework
  - **Why**: Provides robust MVC architecture, dependency injection, and built-in security features
  - **Usage**: Main web application framework handling HTTP requests, routing, and view rendering

### Database & ORM
- **Entity Framework Core 10.0** - Object-Relational Mapping framework
  - **Why**: Simplifies database operations, provides migrations, and enables code-first development
  - **Usage**: Manages database schema, migrations, and data access layer

- **SQL Server** - Relational database management system
  - **Why**: Enterprise-grade database with excellent .NET integration and Azure support
  - **Usage**: Primary database for storing application data (products, orders, users, etc.)

### Authentication & Authorization
- **ASP.NET Core Identity** - Authentication and authorization framework
  - **Why**: Built-in user management, password hashing, role-based access control, and security features
  - **Usage**: Handles user registration, login, password management, and role assignments

### Payment Processing
- **Stripe.net 50.1.0** - Payment processing library
  - **Why**: Industry-standard payment gateway with secure API integration
  - **Usage**: Processes online payments for orders

### Cloud Storage
- **Azure.Storage.Blobs 12.27.0** - Azure Blob Storage client library
  - **Why**: Scalable cloud storage for product images in production environment
  - **Usage**: Stores and retrieves product images in Azure Blob Storage containers

### Frontend Technologies
- **Bootstrap 5** - CSS framework
  - **Why**: Responsive design, pre-built components, and consistent UI
  - **Usage**: Styling and layout of the web application

- **jQuery** - JavaScript library
  - **Why**: DOM manipulation, AJAX requests, and event handling
  - **Usage**: Client-side interactions and dynamic content updates

- **DataTables** - Table enhancement plugin
  - **Why**: Advanced table features (sorting, searching, pagination)
  - **Usage**: Enhanced data display in admin panels

- **SweetAlert2** - Beautiful alert dialogs
  - **Why**: User-friendly confirmation dialogs and notifications
  - **Usage**: Confirmation dialogs for delete operations and user feedback

- **Toastr** - Notification library
  - **Why**: Non-intrusive success/error notifications
  - **Usage**: Displaying temporary success/error messages

- **TinyMCE** - Rich text editor
  - **Why**: WYSIWYG editor for content creation
  - **Usage**: Product description editing

---

## Project Structure

The project follows a clean architecture pattern with clear separation of concerns:

```
CartX/
├── CartX.DataAccess/      # Data access layer
├── CartX.Models/          # Domain models and view models
├── CartX.Utilities/        # Utility classes and helpers
└── CartXWeb/              # Web application (MVC)
```

---

## Configuration

### AppSettings Configuration

The application uses environment-specific configuration files:

- `appsettings.json` - Base configuration (committed to Git)
- `appsettings.Development.json` - Development environment settings
- `appsettings.Production.json` - Production environment settings (excluded from Git)

**⚠️ Important**: All `appsettings.json` files (except `.example` files) are excluded from Git to protect sensitive information.

### Required Configuration Settings

#### Connection Strings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=CartX;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True",
    "AzureBlobStorage": "DefaultEndpointsProtocol=https;AccountName=YOUR_ACCOUNT;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net"
  }
}
```

- **DefaultConnection**: SQL Server connection string for the main database
- **AzureBlobStorage**: Azure Blob Storage connection string (required for production image storage)

#### Stripe Configuration

```json
{
  "Stripe": {
    "SecretKey": "sk_test_YOUR_SECRET_KEY",
    "PublishableKey": "pk_test_YOUR_PUBLISHABLE_KEY"
  }
}
```

- **SecretKey**: Server-side Stripe API key (used for payment processing)
- **PublishableKey**: Client-side Stripe API key (used in frontend for payment forms)

#### Azure Storage Configuration

```json
{
  "AzureStorage": {
    "ContainerName": "product-images"
  }
}
```

- **ContainerName**: Name of the Azure Blob Storage container for storing product images

### Azure App Service Configuration

When deploying to Azure App Service, add these settings in the **Configuration** section:

**Application Settings:**
- `Stripe:SecretKey` = Your Stripe secret key
- `Stripe:PublishableKey` = Your Stripe publishable key
- `AzureStorage:ContainerName` = product-images

**Connection Strings:**
- `ConnectionStrings:DefaultConnection` (Type: SQLAzure)
- `ConnectionStrings:AzureBlobStorage` (Type: Custom)

**Note**: ASP.NET Core automatically reads from Azure App Service configuration (environment variables), so no code changes are required. The configuration system prioritizes environment variables over `appsettings.json` files.

---

## Architecture & Separation of Concerns

The application follows the **Repository Pattern** and **Unit of Work Pattern** to ensure clean separation of concerns:

### Layer Responsibilities

1. **CartXWeb (Presentation Layer)**
   - Controllers handle HTTP requests
   - Views render UI
   - View Components for reusable UI components
   - Services for application-specific logic (storage, etc.)

2. **CartX.DataAccess (Data Access Layer)**
   - Database context and configuration
   - Repository pattern implementation
   - Unit of Work for transaction management
   - Database migrations and seeding

3. **CartX.Models (Domain Layer)**
   - Entity models representing business entities
   - View Models for data transfer between layers
   - Data annotations for validation

4. **CartX.Utilities (Utility Layer)**
   - Static constants and configuration classes
   - Helper classes (EmailSender, StripeSettings)

### Benefits of This Architecture

- **Maintainability**: Clear separation makes code easier to understand and modify
- **Testability**: Each layer can be tested independently
- **Scalability**: Easy to add new features without affecting existing code
- **Reusability**: Repository pattern allows reuse of data access logic

---

## Detailed Component Documentation

### CartX.DataAccess

#### ApplicationDbContext

**Location**: `CartX.DataAccess/Data/ApplicationDbContext.cs`

**Purpose**: Main database context class that inherits from `IdentityDbContext<IdentityUser>` to integrate ASP.NET Core Identity.

**Key Features**:
- Manages all database entities (Categories, Products, Orders, etc.)
- Configures entity relationships and constraints
- Seeds initial data (categories, companies, products) using `HasData()` method
- Integrates with Identity for user management

**DbSets**:
- `Categories` - Product categories
- `Products` - Product catalog
- `ProductImages` - Product image references
- `Companies` - Company information
- `ShoppingCarts` - User shopping cart items
- `ApplicationUsers` - Extended user information
- `OrderHeaders` - Order information
- `OrderDetails` - Order line items

#### DbInitializer

**Location**: `CartX.DataAccess/DbInitializer/DbInitializer.cs`

**Purpose**: Initializes the database on application startup.

**Responsibilities**:
1. **Migration Management**: Automatically applies pending migrations
2. **Role Creation**: Creates default roles (Admin, Employee, Customer, Company)
3. **Admin User Creation**: Creates a default admin user if roles don't exist
   - Email: `admin@cartx.com`
   - Password: `Admin@123`
   - Role: Admin

**Usage**: Called automatically in `Program.cs` during application startup.

#### Repositories

**Location**: `CartX.DataAccess/Repository/`

**Pattern**: Generic Repository Pattern with specific repository interfaces.

**Base Repository** (`Repository<T>`):
- Generic CRUD operations for any entity type
- Supports filtering, eager loading, and tracking control
- Implements `IRepository<T>` interface

**Specific Repositories**:
- `CategoryRepository` - Category-specific operations
- `ProductRepository` - Product-specific operations (with category inclusion)
- `CompanyRepository` - Company management
- `ShoppingCartRepository` - Shopping cart operations
- `ApplicationUserRepository` - User management
- `OrderHeaderRepository` - Order management
- `OrderDetailRepository` - Order detail operations
- `ProductImageRepository` - Product image management

**Why Repository Pattern**:
- Centralizes data access logic
- Makes unit testing easier (can mock repositories)
- Provides consistent data access interface
- Allows easy swapping of data sources

#### UnitOfWork

**Location**: `CartX.DataAccess/Repository/UnitOfWork.cs`

**Purpose**: Manages transactions and coordinates multiple repositories.

**Key Features**:
- Provides access to all repositories through a single interface
- Ensures all repositories share the same `DbContext` instance
- Implements `Save()` method to commit all changes in a single transaction
- Prevents partial updates and maintains data consistency

**Usage Example**:
```csharp
_unitOfWork.Product.Add(product);
_unitOfWork.Category.Update(category);
_unitOfWork.Save(); // Single transaction commits both changes
```

**Why Unit of Work**:
- Ensures transactional consistency
- Reduces database round trips
- Simplifies dependency injection (one service instead of many)

---

### CartX.Models

#### Models

**Location**: `CartX.Models/`

**Purpose**: Domain entities representing business objects.

**Key Models**:

1. **Category**
   - Represents product categories
   - Properties: Id, Name, DisplayOrder

2. **Product**
   - Represents products in the catalog
   - Properties: Id, Title, Author, ISBN, Description, Prices (ListPrice, Price, Price50, Price100)
   - Relationships: Category (many-to-one), ProductImages (one-to-many)

3. **ProductImage**
   - Represents product images
   - Stores image URLs/paths
   - Relationship: Product (many-to-one)

4. **Company**
   - Represents company information
   - Properties: Id, Name, StreetAddress, City, State, PostalCode, PhoneNumber

5. **ShoppingCart**
   - Represents items in user's shopping cart
   - Properties: Id, ProductId, ApplicationUserId, Count
   - Relationships: Product, ApplicationUser

6. **OrderHeader**
   - Represents order information
   - Properties: OrderDate, ShippingDate, OrderTotal, OrderStatus, PaymentStatus, SessionId, etc.
   - Relationships: ApplicationUser, OrderDetails

7. **OrderDetail**
   - Represents individual line items in an order
   - Properties: OrderId, ProductId, Count, Price
   - Relationships: OrderHeader, Product

8. **ApplicationUser**
   - Extends IdentityUser with additional properties
   - Properties: Name, StreetAddress, City, State, PostalCode, PhoneNumber, CompanyId
   - Relationship: Company (optional)

#### ViewModels

**Location**: `CartX.Models/ViewModels/`

**Purpose**: Data transfer objects that combine multiple models or add additional properties for views.

**Key ViewModels**:

1. **ShoppingCartVM**
   - Combines `ShoppingCartList` and `OrderHeader`
   - Used in cart and checkout views

2. **ProductVM**
   - Combines `Product` with `CategoryList` and `ProductImageList`
   - Used in product management views

3. **OrderVM**
   - Combines `OrderHeader` with `OrderDetailList`
   - Used in order detail views

4. **RoleManagementVM**
   - Combines user information with role assignments
   - Used in user management views

**Why ViewModels**:
- Separates view requirements from domain models
- Prevents over-posting attacks
- Allows combining multiple models for complex views
- Provides better control over what data is sent to views

---

### CartX.Utilities

#### SD (Static Details)

**Location**: `CartX.Utilities/SD.cs`

**Purpose**: Contains static constants used throughout the application.

**Constants Defined**:

**Roles**:
- `Role_Admin` - Administrator role
- `Role_Employee` - Employee role
- `Role_Customer` - Customer role
- `Role_Company` - Company role

**Order Status**:
- `StatusPending` - Order is pending
- `StatusApproved` - Order is approved
- `StatusInProcess` - Order is being processed
- `StatusShipped` - Order has been shipped
- `StatusCancelled` - Order is cancelled
- `StatusRefunded` - Order has been refunded

**Payment Status**:
- `PaymentStatusPending` - Payment is pending
- `PaymentStatusApproved` - Payment is approved
- `PaymentStatusDelayedPayment` - Payment approved for delayed payment
- `PaymentStatusRejected` - Payment is rejected

**Session Keys**:
- `SessionCart` - Session key for shopping cart count

**Why Static Constants**:
- Prevents typos in string literals
- Centralizes magic strings
- Enables IntelliSense support
- Makes refactoring easier

#### StripeSettings

**Location**: `CartX.Utilities/StripeSettings.cs`

**Purpose**: Configuration class for Stripe payment settings.

**Properties**:
- `SecretKey` - Server-side Stripe API key
- `PublishableKey` - Client-side Stripe API key

**Usage**: Bound from configuration in `Program.cs` using `builder.Configuration.GetSection("Stripe")`.

#### EmailSender

**Location**: `CartX.Utilities/EmailSender.cs`

**Purpose**: Implements `IEmailSender` interface for sending emails.

**Current Status**: **Not Implemented** - Returns `Task.CompletedTask` without sending emails.

**Interface**: `IEmailSender` from `Microsoft.AspNetCore.Identity.UI.Services`

**Future Implementation**: Can be extended to integrate with:
- SendGrid
- SMTP servers
- Azure Communication Services
- Other email service providers

---

### CartXWeb

#### Areas

**Location**: `CartXWeb/Areas/`

**Purpose**: Organizes controllers and views by functional area.

**Areas Defined**:

1. **Admin Area** (`Areas/Admin/`)
   - **Purpose**: Administrative functionality
   - **Controllers**:
     - `CategoryController` - Category management (CRUD)
     - `ProductController` - Product management (CRUD)
     - `CompanyController` - Company management
     - `OrderController` - Order management and processing
     - `UserController` - User and role management
   - **Access**: Restricted to Admin and Employee roles
   - **Views**: Admin-specific views for managing content

2. **Customer Area** (`Areas/Customer/`)
   - **Purpose**: Customer-facing functionality
   - **Controllers**:
     - `HomeController` - Product browsing and details
     - `CartController` - Shopping cart and checkout
   - **Access**: Public and authenticated users
   - **Views**: Product catalog, product details, shopping cart, checkout

3. **Identity Area** (`Areas/Identity/`)
   - **Purpose**: Authentication and user management
   - **Source**: Scaffolded from ASP.NET Core Identity
   - **Pages**: Login, Register, ForgotPassword, ResetPassword, Manage Account, etc.
   - **Access**: Public (for registration/login) and authenticated users (for account management)

**Why Areas**:
- Organizes large applications into functional modules
- Allows multiple controllers with the same name
- Simplifies routing and navigation
- Improves code organization and maintainability

**Default Route**: The application defaults to the Customer area (`{area=Customer}`).

#### Identity Scaffolding

**Location**: `CartXWeb/Areas/Identity/`

**Purpose**: Customized ASP.NET Core Identity pages.

**How It's Used**:
1. Identity pages were scaffolded using Visual Studio's scaffolding tool
2. Pages were customized to match the application's design
3. Views use Bootstrap styling consistent with the rest of the application

**Scaffolded Pages**:
- `Account/Login.cshtml` - User login
- `Account/Register.cshtml` - User registration
- `Account/ForgotPassword.cshtml` - Password recovery
- `Account/ResetPassword.cshtml` - Password reset
- `Account/Manage/Index.cshtml` - Account management
- `Account/Manage/ChangePassword.cshtml` - Change password
- And more...

**Customization**: Pages were modified to:
- Match the application's Bootstrap theme
- Add custom validation messages
- Integrate with the application's layout
- Add additional user fields (Name, Address, etc.)

#### Storage Services

**Location**: `CartXWeb/Services/`

**Purpose**: Abstracts file storage to support both local and cloud storage.

**Interface**: `IStorageService`
- `UploadFileAsync(string filename, Stream fileStream)` - Uploads a file
- `DeleteFileAsync(string filename)` - Deletes a file
- `GetFileUrlAsync(string filename)` - Gets the URL/path to a file

**Implementations**:

1. **LocalStorageService**
   - **Purpose**: Stores files locally in `wwwroot/images/products/`
   - **Usage**: Development environment
   - **Storage Path**: `wwwroot/images/products/{productId}/{filename}`
   - **Returns**: Relative path (e.g., `\images\products\product-1\guid.jpg`)

2. **AzureBlobStorageService**
   - **Purpose**: Stores files in Azure Blob Storage
   - **Usage**: Production environment
   - **Storage**: Azure Blob Storage container
   - **Returns**: Full Azure Blob URL

**Configuration Logic** (in `Program.cs`):
```csharp
if (!string.IsNullOrEmpty(blobConnectionString) && builder.Environment.IsProduction())
{
    // Use Azure Blob Storage in Production
    builder.Services.AddSingleton<IStorageService, AzureBlobStorageService>();
}
else
{
    // Use Local Storage in Development
    builder.Services.AddScoped<IStorageService, LocalStorageService>();
}
```

**Why Storage Abstraction**:
- Allows switching between storage providers without changing business logic
- Supports different storage strategies for different environments
- Makes testing easier (can mock storage service)
- Enables gradual migration from local to cloud storage

#### View Components

**Location**: `CartXWeb/ViewComponents/`

**Purpose**: Reusable UI components that can be invoked from views.

**View Components**:

1. **ShoppingCartViewComponent**
   - **Purpose**: Displays shopping cart item count in the navbar
   - **Logic**:
     - Gets the current user's shopping cart count
     - Stores count in session for performance
     - Updates session when cart changes
   - **Usage**: Invoked in `_Layout.cshtml` using `@await Component.InvokeAsync("ShoppingCart")`
   - **Returns**: Integer count of items in cart

**Why View Components**:
- Encapsulates view logic and data retrieval
- Reusable across multiple views
- Better separation of concerns than partial views with inline code
- Can be tested independently

#### Views

**Location**: `CartXWeb/Views/` and `CartXWeb/Areas/*/Views/`

**Purpose**: Razor views that render the UI.

**Shared Views** (`Views/Shared/`):
- `_Layout.cshtml` - Main layout template
- `_LoginPartial.cshtml` - Login/logout partial view
- `_Notifications.cshtml` - Toastr notification display

**Area Views**: Each area has its own Views folder with area-specific views.

**View Structure**:
- Uses Razor syntax for server-side rendering
- Integrates with Bootstrap for styling
- Uses partial views for reusable components
- Implements view models for type-safe data binding

#### wwwroot

**Location**: `CartXWeb/wwwroot/`

**Purpose**: Contains static files served directly to clients.

**Directory Structure**:

```
wwwroot/
├── css/
│   └── site.css          # Custom application styles
├── js/
│   ├── site.js           # Custom application scripts
│   ├── product.js        # Product management scripts
│   ├── company.js        # Company management scripts
│   ├── order.js          # Order management scripts
│   └── user.js           # User management scripts
├── images/
│   ├── logo.png          # Application logo (favicon)
│   ├── book.png          # Default product image
│   ├── product/          # Legacy product images (deprecated)
│   └── products/          # Product images organized by product ID
│       └── product-{id}/
│           └── {guid}.{ext}
└── lib/                  # Third-party libraries
    ├── bootstrap/        # Bootstrap CSS/JS
    ├── jquery/           # jQuery library
    └── ...
```

**What's Stored**:
- **CSS Files**: Custom stylesheets
- **JavaScript Files**: Client-side application logic
- **Images**: Static images (logo, default images) and product images
- **Libraries**: Third-party libraries (Bootstrap, jQuery, etc.)

**Static File Serving**: Files in `wwwroot` are served directly by ASP.NET Core's static file middleware.

---

## Image Storage: Development vs Production

### Development Environment

**Storage**: Local file system

**Location**: `wwwroot/images/products/{productId}/{filename}`

**Implementation**: `LocalStorageService`

**How It Works**:
1. Files are uploaded to the local file system
2. Stored in `wwwroot/images/products/{productId}/` directory
3. Database stores relative path: `\images\products\product-1\guid.jpg`
4. Images are served directly by ASP.NET Core static file middleware
5. URLs are relative: `/images/products/product-1/guid.jpg`

**Advantages**:
- No external dependencies
- Fast local access
- Easy to debug
- No additional costs

**Limitations**:
- Not scalable
- Files lost on deployment
- Not suitable for production

### Production Environment

**Storage**: Azure Blob Storage

**Location**: Azure Blob Storage container (`product-images`)

**Implementation**: `AzureBlobStorageService`

**How It Works**:
1. Files are uploaded to Azure Blob Storage
2. Stored in the configured container (`product-images`)
3. Database stores full Azure Blob URL: `https://{account}.blob.core.windows.net/{container}/{filename}`
4. Images are served directly from Azure CDN
5. URLs are absolute: `https://cartxstorage.blob.core.windows.net/product-images/{filename}`

**Advantages**:
- Scalable and reliable
- CDN support for fast global delivery
- Persistent storage (survives deployments)
- Cost-effective for large files
- Automatic backup and redundancy

**Configuration Required**:
- Azure Storage Account
- Connection string in `appsettings.Production.json` or Azure App Service
- Container name: `product-images`

**Automatic Switching**: The application automatically uses Azure Blob Storage in production when:
- `AzureBlobStorage` connection string is configured
- Environment is `Production`
- Falls back to local storage if Azure configuration fails

---

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code
- Azure account (for production deployment)

### Setup Steps

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd CartX
   ```

2. **Configure Database**
   - Update `appsettings.json` with your SQL Server connection string
   - Run migrations:
     ```bash
     dotnet ef database update --project CartX.DataAccess --startup-project CartXWeb
     ```

3. **Configure Stripe** (Optional for development)
   - Add Stripe keys to `appsettings.Development.json`
   - Or use test keys from Stripe dashboard

4. **Run the Application**
   ```bash
   dotnet run --project CartXWeb
   ```

5. **Default Admin Credentials**
   - Email: `admin@cartx.com`
   - Password: `Admin@123`

### Production Deployment

1. **Configure Azure App Service Settings** (see [Configuration](#configuration) section)
2. **Set Environment Variable**: `ASPNETCORE_ENVIRONMENT=Production`
3. **Deploy** using Visual Studio Publish or Azure DevOps

---

## License

This project is proprietary software. All rights reserved.

---

## Author

CartX Development Team
