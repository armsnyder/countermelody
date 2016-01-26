# Countermelody
A rhythm-based tactical RPG by Gregory Elliott, Corey Grief, and Adam Snyder

## Project Setup
There are a couple of necessary steps to take before you can contribute without causing errors 
([source](http://docs.unity3d.com/Manual/ExternalVersionControlSystemSupport.html)).  
1. Download and install [Git Large File Storage](https://git-lfs.github.com/). This allows us to version large 
(>1MB), non-text files without eating up excesive local disk space.
2. Confirm your project settings in Edit->Project Settings->Editor. (These should already be set correctly, but 
just be aware.) __Version Control Mode__ should be set to __Visible Meta Files__, and __Asset Serialization__ should be set to __Force Text__.
3. Create a .gitconfig file, which is not in the repository because it's platform-dependent, and fill it according to 
[these directions](http://docs.unity3d.com/Manual/SmartMerge.html).
4. Add a Smart Merge fallback tool by following 
[these directions](https://www.reddit.com/r/Unity3D/comments/39bdq5/how_to_solve_scene_conflicts_with_unitys_smart/).

## Tips and Guidelines
Feel free to add tips and reminders here. We should be in agreement as far as following certain guidelines so that 
we don't go off on the wrong track and frustrate one another. Also, this first set of tips isn't just coming from 
nowhere, but rather it's distilled from a bunch of Unity tutorials online at 
[lynda.com](https://shib.lynda.com/Shibboleth.sso/InCommon?providerId=urn:mace:incommon:northwestern.edu).  
* Let's use good design principles and keep things modular. Following a roughly Model-View-Controller pattern, the controller (typically a GameManager script) receives events from the view (on-screen game objects), which it uses to update the model (public fields of on-screen game objects). Another example is that UI components should not contain logic but should rather update based on a model and send events when buttons are clicked, etc. 
([This article](http://www.toptal.com/unity-unity3d/unity-with-mvc-how-to-level-up-your-game-development) is a fun read, 
but we don't need to follow their pattern exactly.)
* Practicing a [good branching model](http://nvie.com/posts/a-successful-git-branching-model/) is great. We should keep 
master in a release-state, which for us is the last complete iteration. We'll use the release branch for our in-progress 
iteration.
* Writing [tests](http://blogs.unity3d.com/2014/07/28/unit-testing-at-the-speed-of-light-with-unity-test-tools/) into 
the code is a thing, and so is [debugging](http://docs.unity3d.com/432/Documentation/Manual/Debugger.html). (You can 
attach the game scripts to the Unity Editor process, run them, and then debug the code while the game runs.)
* In order to reduce merge conflict issues, let's try to keep the number of objects in the Hierarchy to a minimum and 
try to use scripts to spawn objects into the scene whenever possible.
* The [MonoBehaviour reference page](http://docs.unity3d.com/ScriptReference/MonoBehaviour.html) is super useful.
* Abstraction is a wonderful thing. For example, with our game we're likely to be mssing with player input a lot. We 
should make sure that adding different controller types (keyboard, guitar, etc.) is easy and that we never hard-code 
controller inputs to behaviors.
* Just because Unity uses the Entity-component system doesn't mean you should never write and extend a base class. 
Sometimes it makes sense for one component to extend another if it shares a lot of similar functionality.
