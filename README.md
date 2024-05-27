# C# Reverse Proxy Demo

## Overview

This is a demo project showcasing the implementation of a reverse proxy using C# and .NET. The project demonstrates how to set up and configure a simple reverse proxy with additional features such as static page caching, in-memory caching, and Redis caching.

## Features

- **Reverse Proxy**: Forward client requests to multiple backend servers.
- **Static Page Caching**: Cache static pages to improve performance.
- **In-Memory Caching**: Use in-memory caching for frequently accessed data.
- **Redis Caching**: Use Redis for distributed caching to improve scalability and reliability.
- **Load Balancing**: Distribute incoming requests across multiple backend servers.
- **SSL Termination**: Provide secure client-server communication.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed
- Redis installed and running (if using Redis caching)
- Basic knowledge of C# and ASP.NET Core


## Configure the reverse proxy:

    Edit the configuration file to define the backend servers and any other settings. For example, modify `appsettings.json`:

    ```json
    {
      "ReverseProxy": {
        "Routes": {
          "route1": {
            "ClusterId": "cluster1",
            "Match": {
              "Path": "{**catch-all}"
            }
          }
        },
        "Clusters": {
          "cluster1": {
            "Destinations": {
              "destination1": {
                "Address": "https://localhost:5001/"
              },
              "destination2": {
                "Address": "https://localhost:5002/"
              }
            }
          }
        }
      },
      "Redis": {
        "SentinelConfiguration": "localhost:26379,localhost:26380,password=4fe0638a10,serviceName=mymaster"
      },
      "CachePath": "data/cache/"
    }
    ```
