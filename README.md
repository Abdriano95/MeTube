# MeTube

## Description
This is our group project for our Object Oriented Design course.

It's a simple YouTube-inspired web application that demonstrates core video streaming concepts—uploading, searching, and playing videos—while focusing on clean design and user-friendly interactions.

Authors: Ali Behrooz, Ronnie, Abdulla Mehdi, Oskar, Sebastian Svensson - Loop Legion


## Table of Contents
- [Documentation](#documentation)
- [Application Architecture](#application-architecture)
- [Screenshots](#screenshots)
- [Sprints](#sprints)
  - [Sprint 1](#sprint-1)
    - [Planning](#planning-1)
    - [Review](#review-1)
    - [Retrospective](#retrospective-1)
  - [Sprint 2](#sprint-2)
    - [Planning](#planning-2)
    - [Review](#review-2)
    - [Retrospective](#retrospective-2)
  - [Sprint 3](#sprint-3)
    - [Planning](#planning-3)
    - [Review](#review-3)
    - [Retrospective](#retrospective-3)
  - [Sprint 4](#sprint-4)
    - [Planning](#planning-4)
    - [Review](#review-4)
    - [Retrospective](#retrospective-4)

## Documentation
This section contains step-by-step instructions for setting up the project on your local computer.

### 1. Restore the database
To ensure you have a clean installation, you first need to restore the database:

#### 1.1 Remove migrations
- Locate and remove all migration files in the project
- Also remove the file `ApplicationDbContextModelSnapShot.cs`

#### 1.2 Remove the database
- Open SQL Server Object Explorer in Visual Studio (View -> SQL Server Object Explorer)
- Go to (localdb)\MSSQLLocalDB
- Expand the "Databases" dropdown
- Expand the "MeTubeDB" dropdown
- Right-click on the database
- Choose "Delete"

#### 1.3 Update the database
- Open the Package Manager Console (Tools -> NuGet Package Manager -> Package Manager Console)
- Right-click on MeTube.Data and select "Set as Start Up Project"
- Run the command: `update-database`
- Navigate to the gear icon next to the Start button and choose "new profile" (or the name you have assigned). This is so you can return to multiple startup projects.

### 2. Update from master

Retrieve the latest version from the master branch:
```bash
git pull origin master
```
Or in Visual Studio, select master and click the Pull button.


### 3. Configure User Secrets

This only needs to be done once on your local machine:

1. Open Developer PowerShell:
   - Go to `Tools -> Command Line -> Developer PowerShell`

2. Navigate to the API project:
   ```bash
   cd MeTube.API
   ```

3. Initialize user secrets:
   ```bash
   dotnet user-secrets init
   ```

4. Configure Azure Storage settings:
   ```bash
   dotnet user-secrets set "AzureStorage:AccountName" "looplegionmetube20250129"
   dotnet user-secrets set "AzureStorage:AccountKey" "xxxx"
   ```
   DISCLAIMER: Contact us for the Azure Storage key!

After following these steps, the project should be properly configured and ready to run on your local machine.

## Application Architecture

## Screenshots

## Sprints

### Sprint 1
#### Planning
- **Sprint Backlog**:
  - *User Story 1*: As a user I should be able to sign up so that I have a user account.
  - *User Story 2*: As a user, I want to log in using basic authentication with my username and password so that I can securely access my account.
  - *User Story 3*: As a user, I want to be able to log out so that I'm logged out.

#### Review


#### Retrospective

### Sprint 2
#### Planning
- **Sprint Backlog**:
  - *User Story 1*: As a user, I want to be able to log out so that I'm logged out.
  - *User Story 2*: As an admin, I want to create, read, update, and delete user accounts so that I can manage users in the system.
  - *User Story 3*: As an authenticated User I want to manage my videos so that I can upload and delete them.
  - *User Story 4*: As an unauthenticated and authenticated user I want to be able to access videos so that I can watch them.
  - *User Story 5*: As a user, I want metadata about the video to be stored in persistent storage when a video is uploaded and removed when the video is deleted, so that I can efficiently manage and retrieve video information.
  - *User Story 6*: As an unauthenticated or authenticated user, I want to view a list of video metadata, so that I can quickly determine which videos are of interest to me before deciding to watch or take further action.

#### Review

#### Retrospective

### Sprint 3
#### Planning
- **Sprint Backlog**:
  - *User Story 1*: As an admin, I want to manage metadata so that I can create read update and delete metadata.
  - *User Story 2*: As an admin I want to manage videos so that I can upload, play, overwrite and delete videos.
  - *User Story 3*: As an authenticated user I want to be able to like a video so that I can show that I enjoyed it.
  - *User Story 4*: As an Admin I want to manage likes for videos so that I can create, read, update and delete likes.
  - *User Story 5*: As an authenticated user I want to be able to post comments on videos so i can share my thoughts and have discussions about them.
  - *User Story 6*: As an admin I want to manage comments for videos so that I can edit or delete them.

#### Review

#### Retrospective

### Sprint 4
#### Planning
- **Sprint Backlog**:
  - *User Story 1*: As an authenticated user, I want to view my history so I can view the name and see when I have seen the videos.
  - *User Story 2*: As an admin, I want to manage history so that I can create, read, update and delete videos in history.
  - *User Story 3*: As a user, I want videos to be recommended to me based on a simple recommendation algorithm so that I can discover and enjoy content that matches my interests.

#### Review

#### Retrospective
