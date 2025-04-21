# Microsoft Fabric RTI Sample Workload

Welcome to the Microsoft Fabric RTI Sample Workload repository. This repository contains an application hosting a sample Microsoft Fabric workload. We will be using this repository to demonstrate key scenarios and features of the Microsoft Fabric RTI (Real-Time Intelligence) platform.

## Table of Contents

- [Features](#features)
- [Disclaimer](#disclaimer)
- [Quick Start](#quick-start---running-the-sample-workload-in-a-local-development-environment)
- [Sample Item Creation](#sample-item-creation)
- [Eventstream](#eventstream)
- [Trademarks](#trademarks)

## Features

The sample demonstrates the following Microsoft Fabric RTI capabilities:

- **Eventhouse and KQL Database**: Real-time data ingestion and querying
- **Eventstream**: Real-time data streaming and processing engine (coming soon)
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

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general). Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks or logos are subject to those third-party's policies.
