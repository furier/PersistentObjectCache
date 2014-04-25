PersistentObjectCache
=====================

Persistent Object Cache serializes objects and store them to disk as json. When retrieving objects cache time is validated, if cache time has been exceeded null is returned otherwise the object.
