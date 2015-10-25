ResourceChangeEvent
===================

(C# WPF) Subscribe event handlers to resource change event

Description
===========

Introduces a Behavior, that triggers an event on each change of a specific `WPF` resource.

Usage
=====

Add following behavior to your `XAML` file:
```
  <i:Interaction.Behaviors>
    <resourceChangeEvent:ResourceChangeEventBehavior 
      Resource="{DynamicResource VariableFontSize}" 
      ResourceChanged="OnResourceChanged_VariableFontSize"/>
  </i:Interaction.Behaviors>
```

Where 
- `VariableFontSize` is the name of the resource that you want to monitor
- `OnResourceChanged_VariableFontSize` is the event-handler in your code behind, that handle the event

For more details, see the example Project.

Installation
============

you have the choice:
- Add the project 'ResourceChangeEvent' to your solution
- Copy the files of the project 'ResourceChangeEvent' to your Util project
- Compile the solution 'ResourceChangeEvent' and copy the dll `ResourceChangeEvent.dll`

