# PC Build Assistant

## Overview

PC Build Assistant is an AI-powered web application that provides personalized PC build recommendations based on user preferences and requirements. The application uses Azure OpenAI services to generate intelligent hardware recommendations, taking into account budget, purpose, performance requirements, and compatibility considerations.

## System Architecture

### Backend Architecture
- **Framework**: ASP.NET Core 8.0 with C#
- **Architecture Pattern**: MVC with API controllers and Blazor Server components
- **Hosting**: Configured to run on port 5000 with cross-platform compatibility
- **AI Integration**: Azure OpenAI service for intelligent PC build recommendations

### Frontend Architecture
- **UI Framework**: Blazor Server with Bootstrap 5.3.0 for responsive design
- **Component Structure**: Razor components with server-side rendering
- **Styling**: Custom CSS with Bootstrap integration and Font Awesome icons
- **JavaScript**: Minimal client-side JavaScript for enhanced UX (tooltips, file downloads, smooth scrolling)

### Service Layer Architecture
- **Dependency Injection**: Built-in ASP.NET Core DI container
- **Service Interfaces**: Clean separation between service contracts and implementations
- **HTTP Client**: Configured for external API calls and service integrations

## Key Components

### Core Services
1. **AzureOpenAIService**: Integrates with Azure OpenAI to generate PC build recommendations
2. **PCBuildService**: Orchestrates build generation and handles PDF/text export functionality
3. **HttpClient**: Registered for external service communications

### Models
- **PCBuildRequest**: Encapsulates user preferences and build requirements
- **PCBuildResponse**: Contains complete build recommendations with components and metadata
- **UserPreferences**: Detailed user requirements including budget, purpose, and technical preferences
- **ComponentInfo**: Individual hardware component details with pricing and compatibility information

### Controllers
- **PCBuildController**: RESTful API endpoints for build generation and file downloads

### Blazor Components
- **Home Page**: Landing page with feature highlights and call-to-action
- **BuildQuestionnaire**: Interactive form for collecting user preferences
- **BuildResults**: Displays generated PC build with download options
- **MainLayout/NavMenu**: Application shell and navigation

## Data Flow

1. **User Input Collection**: BuildQuestionnaire component collects user preferences through a comprehensive form
2. **API Request**: Form submission triggers API call to PCBuildController
3. **AI Processing**: AzureOpenAIService processes requirements and generates recommendations using GPT-4
4. **Response Processing**: PCBuildService parses AI response and structures it into PCBuildResponse model
5. **Result Display**: BuildResults component renders the complete build recommendation
6. **Export Options**: Users can download results as PDF or text files

## External Dependencies

### AI Services
- **Azure OpenAI**: Primary AI service for generating PC build recommendations
- **GPT-4**: Default deployment model for intelligent recommendations

### Frontend Libraries
- **Bootstrap 5.3.0**: UI framework and responsive grid system
- **Font Awesome 6.4.0**: Icon library for enhanced visual design

### Document Generation
- **iText7.pdfhtml**: PDF generation capability for build reports

### Configuration
- Environment variables and appsettings.json for Azure OpenAI configuration
- Support for development and production environment configurations

## Deployment Strategy

### Container Compatibility
- Application configured to bind to all interfaces (0.0.0.0:5000) for container deployment
- Cross-platform .NET 8.0 support for various hosting environments

### Environment Configuration
- Flexible configuration system supporting environment variables and JSON configuration
- Separate development and production settings
- Secure handling of API keys and sensitive configuration

### Static Assets
- wwwroot directory contains all static files (CSS, JavaScript, images)
- CDN integration for external libraries to improve performance

## User Preferences

Preferred communication style: Simple, everyday language.

## Recent Changes

- June 26, 2025: Removed sidebar navigation and implemented full-width layout with top header
- June 26, 2025: Fixed HTTP client base address configuration for Blazor Server API calls
- June 26, 2025: Updated UI styling to use complete screen width and improved navigation with dropdown menu

## Changelog

Changelog:
- June 26, 2025. Initial setup
