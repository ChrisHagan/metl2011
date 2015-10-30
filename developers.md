---
layout: public
---

#Content rendering

Content comes to the client in a preparser.  This contains a full history, separated by content types.

A collaboration page is parameterised to a slide.  It puts that slide into its datacontext.

The collaboration page houses a presentation space.  That presentation space inherits the datacontext.

The presentation space traverses the preparser.  For each content type, it injects all the content into its canvas stack.  The canvas stack is a renderer.  It is unaware of the reasons it receives the content, or any logical location within MeTL.
