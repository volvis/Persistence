# Persistence components

This package contains scripts that help create persistent changes during gameplay.

Data is tied to scenes via hashes generated from scenes' names, and to objects by incremental integers.
This is so that there are no external data sources outside individual scenes to manage but comes with a few caveats.

* Components will warn if scene names in project may cause conflicts in the system. Renaming scenes will fix the issues.
* Components will also warn if two objects in the same scene share the same integer id and offer to find the next free integer.

System has mechanisms to write out and restore its internal state (checkpoints, file saves) but this is still WIP.

## PersistentTriggerState

Trigger an event in object that is retriggered when reloading scene. Can be permanent (collected items) or reset after
changing scenes a number of times (destroyed enemies).

Component contains two separate UnityEvents. When the state is triggered first time, OnTrigger() is invoked.
OnTriggeredBefore() is called when scene is reloaded.

## PersistentDirectorState

Controls a Playable Director instance in a persistent fashion. Triggering the state plays the timeline.
When returning to the scene, timeline jumps to the final frame during OnEnable.

Useful for making cut scenes that result in permanent change in scene.