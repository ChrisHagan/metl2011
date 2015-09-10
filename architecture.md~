---
layout: public
---

* [Concepts][#concepts]
* [Configurability][#configurability]
* [Entities][#entities]
* [Integration][#integration]

#Concepts

MeTL at its heart is a live message exchange engine, with all messages being persisted for later retrieval.

The default messaging mechanism is XMPP, and most of the MeTL messaging components use that protocol.

The persistence mechanism of choice is a filestore.  Alternative components include MongoDB and SQL (each of these is under development).

Clients to the MeTL system operate in a room metaphor - each message is sent to a specific space, and only peers who are connected and in that space will receive it.

Messages can be user level, and be visible to a human user, or system level and used to coordinate the clients.

#Configurability

An installed MeTL system must configure one of each of:

* A persistence engine
* A messaging engine
* An authentication provider
* An authorization provider

[configurationArchitecture]: images/configurationArchitecture.png "Configuration architecture"
![A component diagram of MeTL, demonstrating configuration points][configurationArchitecture]

#Entities

#Integration
