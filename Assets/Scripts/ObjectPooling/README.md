<h1> Object pooling helper class </h1>
<br>
<h3> These scripts help you set up an object pooling system in your game. It's pretty simple to use, let me show you how: </h3>

<br>


<h3> 1. Initializing the pool </h3>
<a> As you can see in the image bellow, you can initialize the pool by creating a new pool and then calling it in the start. </a>

![Init Image](https://github.com/LesserKnownThings/UnityLibrary/blob/object_pooling/Assets/Scripts/ObjectPooling/Images/Initialization.PNG)

 You'll also have to add the namespace that the pool class is in **using UnityHelper.Pooling;**
 
 <h3> 2. Before you start the game </h3>
 You will have to add the objects that you want the pool to instantiate when it starts. As you can see in the image bellow, the pool object looks like this in the editor:
 
 ![Look Image](https://github.com/LesserKnownThings/UnityLibrary/blob/object_pooling/Assets/Scripts/ObjectPooling/Images/PoolObjectLook.PNG)
 
 The pool contains a List of objects that you want to instantiate in the scene. The objects are of type **PoolObject** which you can see here:
 
  ![List Image](https://github.com/LesserKnownThings/UnityLibrary/blob/object_pooling/Assets/Scripts/ObjectPooling/Images/PoolObject.PNG)
  
  The **obj_to_pool** is the gameobject that you want to instantiate and the **amount_to_pool** is the amount of objects that you want to instantiate.
  
  <h3> 3. Spawning the objects in the scene </h3>
  Spawning an object is very easy, the **Pool** class comes with a function called **Spawn_Item(Type type, Vector3 position, Quaternion direction)** it returns a **Component** so you can get direct reference to the object. Here's how you do it:
  
  ![Spawn Image](https://github.com/LesserKnownThings/UnityLibrary/blob/object_pooling/Assets/Scripts/ObjectPooling/Images/SpawnObject.PNG)
  
  In my example I have a custom Object created by me called **CubeObj** it doesn't do much, but it helps with the example. So when you spawn an item in the scene you have to give the function the following parameters:
  
  >**Type** the typeof object that the list needs to look for and spawn it in the scene
  
  >**Position** the position where the item should spawn
  
  >**Rotation** the rotation of the item
  
   One more thing to remember. Pool has a function to bring back the objects to the pool, but it requires the object as a parameter. That's why I made an interface called **IPoolActions** that comes with **ReturnObjectToPool** void and will help you return the items to the pool. It's pretty easy, to return an item you simply have to **gameobject.SetActive(false)** and also reset its position **transform.position = Vector3.zero**. 
