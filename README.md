# Microsoft Fabric RTI Sample Workload

Welcome to the Microsoft Fabric RTI Sample Workload repository. This repository contains an application hosting a sample Microsoft Fabric workload. We will be using this repository to demonstrate key scenarios and features of the Microsoft Fabric RTI (Real-Time Intelligence) platform.

## Table of Contents

- [Features](#features)
- [Disclaimer](#disclaimer)
- [Quick Start](#quick-start---running-the-sample-workload-in-a-local-development-environment)
- [Sample Item Creation](#sample-item-creation)
- [Eventstream](#eventstream)
- [Eventhouse and KQL database](#eventhouse-and-kql-database)
- [Trademarks](#trademarks)

## Features

The sample demonstrates the following Microsoft Fabric RTI capabilities:

- **Eventhouse and KQL Database**: Real-time data ingestion and querying
- **Eventstream**: Real-time data streaming and processing engine
- **Activator**: Real-time data processing and alerting engine (coming soon)

Our repo is based on the [Microsoft-Fabric-workload-development-sample](https://github.com/microsoft/Microsoft-Fabric-workload-development-sample) repository and the official documentation [Microsoft Fabric Workload Development Kit](https://learn.microsoft.com/en-us/fabric/workload-development-kit/development-kit-overview).

## Disclaimer

This repository is a stripped version of the [Microsoft-Fabric-workload-development-sample](https://github.com/microsoft/Microsoft-Fabric-workload-development-sample) repository. While this repository is intended to demonstrate the capabilities of the Microsoft Fabric RTI platform, it might not be up-to-date with the latest changes in the original repository. For the various features and capabilities of a Workload, please refer to the original repository.

## Quick Start - Running the Sample Workload in a Local Development Environment

Please refer to the [Microsoft Fabric Workload Development Kit Quickstart Guide](https://learn.microsoft.com/en-us/fabric/workload-development-kit/quickstart-sample) documentation for detailed instructions on how to set up your development environment. Since this repository is based on the [Microsoft-Fabric-workload-development-sample](https://github.com/microsoft/Microsoft-Fabric-workload-development-sample) repository, the instructions are the same.

## Sample Item Creation

When creating a sample item in the Fabric UX portal, the backend will create the following RTI items:

1. **Eventhouse**
2. **KQL Database** - containing a table named **IotData** with a few sample records
3. **Eventstream** - configured with a custom endpoint as the source and the KQL database as the destination

Details about these RTI items will be stored in the sample item's metadata and included in the response of the item **GET** request. This metadata will be utilized by the frontend extension for various requests and functionalities.

## Eventstream

In our sample, the Eventstream is configured to ingest incoming events as records into the KQL database. This is also known as a data connection. The Eventstream configuration is as follows:

1. **Source** - A custom endpoint that accepts custom events in formats such as JSON
2. **Destination** - The KQL database, where the processed events are transformed and stored as records

### Data Flow Process

1. **Frontend** - On the Eventstream tab, generate events and send them as a request to the backend **EventstreamController**
2. **EventstreamController** - Use the Eventstream public API to retrieve a connection string for the Eventstream source custom endpoint
3. **EventhubClient** - Use the connection string and the Azure Event Hub SDK to send events to the Eventstream source endpoint
4. **Eventstream** - Once the Eventstream receives the events, it processes and ingests them as records into the KQL database

> **Note**: There may be a delay between the time events are sent to the Eventstream and when they are ingested into the KQL database and become available for querying.

### Useful Links

Here are some helpful resources for working with Eventstream and related components:

- [Create Eventstream](https://learn.microsoft.com/en-us/rest/api/fabric/eventstream/items/create-eventstream?tabs=HTTP) - Learn how to create an Eventstream using the REST API
- [Eventstream Topology](https://learn.microsoft.com/en-us/rest/api/fabric/eventstream/topology) - Understand the topology and structure of an Eventstream
- [Eventhouse Destination](https://learn.microsoft.com/en-us/fabric/real-time-intelligence/event-streams/add-destination-kql-database?pivots=enhanced-capabilities) - Guide to adding an Eventhouse as a destination for Eventstream
- [Microsoft Fabric Documentation](https://learn.microsoft.com/en-us/fabric/) - Official Microsoft Fabric documentation

## Eventhouse and KQL Database

In our sample, the Eventhouse and its KQL database provide real-time data ingestion and querying capabilities. During sample item creation, the Eventhouse is created via the Fabric API, and a KQL database is automatically provisioned with it. Using the data plane API, a table is created and populated with sample records.

The frontend application includes multiple tabs that demonstrate key data plane operations for the KQL database, including data querying, exploration, and management capabilities. These demonstrations showcase how to effectively interact with and utilize KQL databases in your Fabric RTI applications.

### KQL Database Data Plane API

This sample demonstrates various data plane operations on KQL databases, including:

- Querying data
- Ingesting data
- Executing control commands (configuring tables, schemas, update policies, retention settings, etc.)

All these operations are triggered using the [Kusto REST API](https://learn.microsoft.com/en-us/kusto/api/rest/?view=microsoft-fabric) either in its pure REST form or via one of the available SDKs, such as the [.NET SDK](https://learn.microsoft.com/en-us/kusto/api/netfx/about-the-sdk?view=microsoft-fabric) used in our backend implementation.

### Required Delegate Permissions

To utilize the Kusto REST API, a token with valid Kusto scope is required. This requires the backend to exchange the user's Fabric token for a Kusto token. To enable this process:

1. During workload application [authentication setup](https://learn.microsoft.com/en-us/fabric/workload-development-kit/authentication-tutorial), add the **user_impersonation** delegated permission for 'Azure Data Explorer'.
1. When users interact with the workload and consent to the application, they must also accept the user impersonation permission for Azure Data Explorer.

### Authorization and Permissions

In addition to token exchange with the Kusto audience, the original caller must have appropriate permissions on the database to execute queries or management operations. For more details, please refer to [Security roles overview](https://learn.microsoft.com/en-us/kusto/management/security-roles?view=microsoft-fabric).

In the context of Fabric, permissions operate at two levels:

- **Security Role on Cluster/Database/Table**: The user is listed as a principal (or a member of a security group) with an appropriate security role on the cluster, database, or table.

- **Fabric Workspace Permissions**: The user has permission on the Fabric Workspace containing the Eventhouse or KQL database item:
  - Users with Viewer access on the workspace receive reader permission on the Eventhouse/KQL database
  - Users with Admin access on the workspace receive Admin permission on the Eventhouse/KQL database

### KQL Query

Below is an overview of the KQL query execution flow, demonstrating how user-initiated queries in the frontend are processed through the application layers and executed against the Eventhouse KQL database:

1. **Frontend** interacts with the frontend page, types in a query, and clicks the execute button.
1. **Frontend** sends an HTTP POST request to the 'KqlDatabases/query' endpoint on the Backend's **KqlDatabaseController**.
1. **KqlDatabaseController** validates the user token and exchanges it for a Kusto audience token.
1. Backend executes a KQL query request on the Eventhouse using **KustoClientService**.cs targeting the KQL Database query URI.
1. Eventhouse processes the query and returns the dataset results.
1. **KqlDatabaseController** formats the results and sends them back to the **frontend**.
1. **Frontend** displays the results in a table format.

> **Notice:**  
>
> 1. If token exchange fails due to "AADSTS65001: The user or administrator has not consented to use the application with ID xxxxx", make sure the user consented to the required scope of 'Azure Data Explorer'.  
> 2. The above query flow represents queries that complete within 30 seconds. For longer-running queries, an additional Long Running Operations (LRO) implementation is required.

### KQL Management Command Execution

Executing a KQL management command follows a similar flow to the query execution process described above:

1. **Frontend** interacts with the frontend page, types in a control command, and clicks the execute button.
1. **Frontend** sends an HTTP POST request to the 'KqlDatabases/mgmt' endpoint on the Backend's **KqlDatabaseController**.
1. **KqlDatabaseController** validates the user token and exchanges it for a Kusto audience token.
1. Backend executes a KQL management command on the Eventhouse using **KustoClientService**.cs targeting the KQL Database query URI.
1. Eventhouse processes the command and returns the results.
1. **KqlDatabaseController** formats the results and sends them back to the **Frontend**.
1. **Frontend** displays the results in a table format.

#### Key Differences Between KQL Queries and Management Commands

1. **Syntax**: Management commands always begin with a dot (.)  
   Examples:  
   - `.show tables`  
   - `.create table MyLogs (Level:string, Timestamp:datetime)`

2. **API Endpoint**: The [Kusto REST API](https://learn.microsoft.com/en-us/kusto/api/rest/?view=microsoft-fabric) uses different endpoints:
   - `/query` for executing queries
   - `/mgmt` for executing management commands

3. **Permission Requirements**: Different operations require different permission levels:
   - Queries typically require reader permission
   - Management operations vary by command type:
     - `.show` operations require reader permission
     - `.create`, `.alter`, and other modification operations require admin-level permissions

### Queued Ingestion

Queued ingestion provides a direct data ingestion into a KQL database without requiring an Eventstream. This method offers several advantages:

- **High Throughput**: Optimized for efficient data processing by batching data based on ingestion properties
- **Data Optimization**: Small batches are automatically merged and optimized to enable fast query performance
- **Reliability**: Built-in retry mechanisms protect against transient failures
- **Data Consistency**: Uses 'at least once' messaging semantics to ensure no data is lost during ingestion

By default, queued ingestion batches data until one of these thresholds is reached:

- 5 minutes elapsed time
- 1000 items collected
- 1 GB total size accumulated

The maximum data size for a single queued ingestion command is 6 GB.

#### Data Source Options

You can provide data for ingestion through several methods:

- A file path in a local directory
- A link to an external file (such as an Azure blob with public access or a SAS token)
- A direct string containing the content to ingest.

#### Prerequisites

Before using queued ingestion, you must:

- [Create a table](https://learn.microsoft.com/en-us/kusto/management/create-table-command?view=microsoft-fabric) that will receive the ingested data
- (Optional) Configure an [ingestion batching policy](https://learn.microsoft.com/en-us/kusto/management/batching-policy?view=microsoft-fabric)
- (Optional) Set up [ingestion mapping](https://learn.microsoft.com/en-us/kusto/management/mappings?view=microsoft-fabric) to define how source data maps to table columns

#### Flow Overview

1. **Frontend** interacts with the frontend page, generating data to be ingested into the KQL database.
1. **Frontend** sends an HTTP POST request to the 'KqlDatabases/queuedIngest' endpoint on the Backend's **KqlDatabaseController**.
1. **KqlDatabaseController** validates the user token and exchanges it for a Kusto audience token.
1. Backend sends a queued ingestion request to the Eventhouse using **KustoClientService**.cs targeting the KQL Database ingestion URI.
1. Eventhouse processes the request and ingests the data.
1. **Frontend** displays indication for successful or failed ingestion.
1. After successful ingestion, the data becomes available for querying. Note that there may be a delay until the data appears in query results, depending on your configured ingestion batching policy.

#### Additional Resources

- **[Creating Applications with Queued Ingestion](https://learn.microsoft.com/en-us/kusto/api/get-started/app-queued-ingestion?view=azure-data-explorer&tabs=app%2Ccsharp)** - Step-by-step guide to building applications that use queued ingestion
- **[Supported Data Formats](https://learn.microsoft.com/en-us/azure/data-explorer/ingestion-supported-formats)** - Comprehensive list of file formats supported by KQL database ingestion
- **[Ingestion Property Reference](https://learn.microsoft.com/en-us/kusto/ingestion-properties?view=azure-data-explorer&preserve-view=true)** - Detailed documentation of all available ingestion properties and their usage

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general). Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks or logos are subject to those third-party's policies.
