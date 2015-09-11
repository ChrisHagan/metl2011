---
layout: public
---

<!-- markdown-toc start - Don't edit this section. Run M-x markdown-toc/generate-toc again -->
**Table of Contents**

- [Concepts](#concepts)
- [Configurability](#configurability)
- [Entities](#entities)
    - [Structure](#structure)
        - [Conversations](#conversations)
        - [Slides](#slides)
        - [Quizzes](#quizzes)
    - [Canvas content](#canvas-content)
        - [Ink](#ink)
        - [Text](#text)
        - [Images](#images)
    - [Submissions](#submissions)
- [Integration](#integration)

<!-- markdown-toc end -->

#Concepts

MeTL at its heart is a message hub, with all messages being persisted for later retrieval.

Messages are sent to a specific space (a chatroom, continuing the MUC metaphor from XMPP), and only people who are in that room will hear the message.

Messages can be user level, and be visible to a human user, or system level and used to coordinate clients behaviour.

All messages which have ever been through a room are retained, and can be replayed in order.  Server side mechanisms optimize the results so that, for instance, a sentence which was published, moved and then later deleted does not show up in the client history at all.

[Conversations](#conversations) are structured as a collection of [slides](#slides) and some metadata.  This is similar to the structure of a PowerPoint presentation, which enables some interoperability.

A slide is a room.

Each user has a private room on each slide.

Each conversation has a conversation global room.  Quizzes, submissions and attachments use this space, as they are not specific to a slide.

A server global room carries configuration data to all connected clients (when a conversation is shared differently, for instance, this is broadcast globally in case that conversation needs to be added or removed from a search result).

The default messaging mechanism is XMPP (Jabber), and most of the MeTL messaging components use that protocol.  XMPP is so pervasive in the MeTL architecture that conversations and slides refer to themselves using a jid (Jabber ID).

The default persistence mechanism is a filestore.  Alternative components include MongoDB and SQL (each of these is under development).

#Configurability

An installed MeTL system must configure one of each of:

* A persistence engine
* A messaging engine
* An authentication provider
* An authorization provider

[configurationArchitecture]: images/configurationArchitecture.png "Configuration architecture"
![A component diagram of MeTL, demonstrating configuration points][configurationArchitecture]

#Entities

Implementation for these entities can be found inside the [MeTL dependencies repository](https://github.com/StackableRegiments/dependencies/blob/master/MeTLData/MeTLData/src/main/scala/metlDataTypes.scala).  XML and JSON serializers are available within this repository as dependencies.  This section presents them in non-compliant JSON Schema.

All entities share the following attributes:

~~~javascript
{
  server:{type:ServerConfiguration},       
  author:{type:String},
  timestamp:{type:Long}
}
~~~

A ServerConfiguration indicates the top level location of the content.  This can be used to differentiate between organizations, or org units, or separate installations.

The timestamp indicates the time at which the server processed the content.  It is not dependent on the user's locale or clock.

The user is identified as a simple string UID.

##Structure 

###Conversations

A Conversation is the top level of content in MeTL.  It is created by a user, and that user retains ownership rights over it.  A Conversation is similar to a PowerPoint presentation in structure.

~~~javascript
{
  author:{type:String},
  lastAccessed:{type:Int},
  slides:{type:Array},
  subject:{type:String},
  tag:{type:String},
  jid:{type:Int},
  title:{type:String},
  created:{type:String},
  permissions:{type:Permission},
  configName:{type:String},
}
~~~

###Slides

A slide is a room level content space.  When a user enters a slide, their client replays the history of content on that slide.

~~~javascript
{
  id:{type:Int},
  author:{type:String},
  index:{type:Int},
}
~~~

###Quizzes

A quiz has an author, a question and some answers to choose from.

~~~javascript
{
  type:"quiz",
  created:{type:Int},
  question:{type:String},
  id:{type:String},
  isDeleted:{type:Bool},
  options:{type:Array,items:{type:Option}}
}
~~~

Quiz options are the available answers to choose from.

~~~javascript
{
  type:"quizOption",
  name:{type:String},
  text:{type:String},
  correct:{type:Bool},
  color:{type:Color}
}
~~~

Quiz responses are an instance of an answer, tying a quiz response to a user.

~~~javascript
{
  type:"quizResponse",
  answer:{type:String},
  answerer:{type:String),
  id:{type:String})
)
~~~

##Canvas content

All objects which appear visually on the main canvas have the following attributes in common:

~~~javascript
{
  target:{type:String},
  privacy:{type:Privacy},
  slide:{type:String},
  identity:{type:String},
  scaleFactorX:{type:Double},
  scaleFactorY:{type:Double}
}
~~~

Where a target is the location on which the content should appear.  The private note space, or the public canvas, for instance, are locations.

A Privacy can be Private or Public.

The identity of the element is a hash of its significant attributes, enabling simple comparison by value or identity when deduplication or modification is required.

###Ink

Ink is described in single strokes, which represent pressure variable paths.

~~~javascript
{
  type:"ink"
  bounds:{type:Array},
  checksum:{type:Double},
  startingSum:{type:Double},
  points:{type:,array","items:Point},
  color:{type:Color},
  thickness:{type:Double},
  isHighlighter:{type:Bool}
}
~~~

Where a Point is a triplet of doubles pulled off the point string:

~~~javascript
{
  x:{type:Double},
  y:{type:Double},
  thickness:{type:Double}
}
~~~

As a performance optimisation this is actually transmitted as "x y t x y t...", but the conceptual model is that of a sequence of pressure aware Points.

###Text

###Images

##Submissions

A submission is an image and a message.

~~~javascript
{
  title:{type:String},
  slide:{type:Int},
  url:{type:String},
  bytes:{type:Array,items:Byte},
  blacklist:{type:Array,items:String}
}
~~~

Where the blacklist specifie users who are not permitted to view this submission.

#Integration

All work in all conversations by a user can be retrieved over HTTP:

1. Given a particular username: /conversationSearch?query={username}
1. For each of the conversation jids returned: /detailsFor/{jid}
1. For each of the returned details.slides: /fullHistory/{slideJid}
