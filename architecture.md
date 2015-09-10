---
layout: public
---

* [Concepts](#concepts)
* [Configurability](#configurability)
* [Entities](#entities)
* [Integration](#integration)

#Concepts

MeTL at its heart is a live message exchange engine, with all messages being persisted for later retrieval.

The default messaging mechanism is XMPP, and most of the MeTL messaging components use that protocol.

The default persistence mechanism is a filestore.  Alternative components include MongoDB and SQL (each of these is under development).

Clients to the MeTL system operate in a room metaphor - each message is sent to a specific space, and only peers who are connected and in that space will receive it.

Messages can be user level, and be visible to a human user, or system level and used to coordinate clients behaviour.

All messages which have ever been through a room are retained, and can be replayed in order.  Server side mechanisms optimize the results so that, for instance, a sentence which was published, moved and then later deleted does not show up in the client history at all.

[Conversations](#conversations) are structured as a collection of [slides](#slides) and some metadata.

This is similar to the mental model of a PowerPoint presentation.

#Configurability

An installed MeTL system must configure one of each of:

* A persistence engine
* A messaging engine
* An authentication provider
* An authorization provider

[configurationArchitecture]: images/configurationArchitecture.png "Configuration architecture"
![A component diagram of MeTL, demonstrating configuration points][configurationArchitecture]

#Entities

Implementation for these entities can be found inside the [MeTL dependencies repository](https://github.com/StackableRegiments/dependencies/blob/master/MeTLData/MeTLData/src/main/scala/metlDataTypes.scala).  XML and JSON serializers are available within as dependencies.  This section represents them in the form of Scala [lift-json](https://github.com/lift/lift/tree/master/framework/lift-base/lift-json/) case classes.

The 'toJsObj' method features in some of these definitions.  It is a wrapper method which simply adds a 'type' field to the object:

```scala
protected def toJsObj(name:String,fields:List[JField]) = Stopwatch.time("JsonSerializer.toJsObj", () => {
    JObject(JField("type",JString(name)) :: fields)
})
```

##Conversations

A Conversation is the top level of content in MeTL.  It is created by a user, and that user retains ownership rights over it.  A Conversation is similar to a PowerPoint presentation in structure.

```scala
JObject(List(
      JField("author",JString(input.author)),
      JField("lastAccessed",JInt(input.lastAccessed)),
      JField("slides",JArray(input.slides.map(s => fromSlide(s)).toList)),
      JField("subject",JString(input.subject)),
      JField("tag",JString(input.tag)),
      JField("jid",JInt(input.jid)),
      JField("title",JString(input.title)),
      JField("created",JString(input.created)),
      JField("permissions",fromPermissions(input.permissions)),
      JField("configName",JString(input.server.name))
    ))
```

##Slides

A slide is a room level content space.  When a user enters a slide, their client replays the history of content on that slide.

```scala
JObject(List(
      JField("id",JInt(input.id)),
      JField("author",JString(input.author)),
      JField("index",JInt(input.index))
    ))
```

##Users

A user is a unique entity within MeTL, who must be authenticated to enter a space.

##Quizzes

A quiz has an author, a question and some answers to choose from.

```scala
toJsObj("quiz",List(
      JField("created",JInt(input.created)),
      JField("question",JString(input.question)),
      JField("id",JString(input.id)),
      JField("isDeleted",JBool(input.isDeleted)),
      JField("options",JArray(input.options.map(o => fromQuizOption(o))))
```

Quiz options are the available answers to choose from.

```scala
toJsObj("quizOption",List(
      JField("name",JString(input.name)),
      JField("text",JString(input.text)),
      JField("correct",JBool(input.correct)),
      JField("color",fromColor(input.color).asInstanceOf[JValue])
    ))
```

##Ink

##Text

##Images

##Submissions

#Integration
