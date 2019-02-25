# Persistence components

This package contains scripts that help create persistent changes during gameplay.

## PersistentTriggerState

Trigger an event in object that is retriggered when reloading scene. Can be permanent (collected items) or reset after
changing scenes a number of times (destroyed enemies).

Component contains two separate UnityEvents. When the state is triggered first time, OnTrigger() is invoked.
OnTriggeredBefore() is called when scene is reloaded.