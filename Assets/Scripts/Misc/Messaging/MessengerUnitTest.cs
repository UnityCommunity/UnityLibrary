#pragma warning disable 0168

// MessengerUnitTest.cs v1.0 by Magnus Wolffelt, magnus.wolffelt@gmail.com
// 
// Some functionality testing of the classes in Messenger.cs.
// A lot of attention is paid to proper exception throwing from the Messenger.
 
using System;
 
class MessengerUnitTest {
 
	private readonly string eventType1 = "__testEvent1";
	private readonly string eventType2 = "__testEvent2";
 
	bool wasCalled = false;
 
	public void RunTest() {
		RunAddTests();
		RunBroadcastTests();
		RunRemoveTests();
		Console.Out.WriteLine("All Messenger tests passed.");
	}
 
 
	private void RunAddTests() {
		Messenger.AddListener(eventType1, TestCallback);
 
		try {
			// This should fail because we're adding a new event listener for same event type but a different delegate signature
			Messenger<float>.AddListener(eventType1, TestCallbackFloat);
			throw new Exception("Unit test failure - expected a ListenerException");
		} catch (MessengerInternal.ListenerException e) {
			// All good
		}
 
		Messenger<float>.AddListener(eventType2, TestCallbackFloat);
	}
 
 
	private void RunBroadcastTests() {
		wasCalled = false;
		Messenger.Broadcast(eventType1);
		if (!wasCalled) { throw new Exception("Unit test failure - event handler appears to have not been called."); }
		wasCalled = false;
		Messenger<float>.Broadcast(eventType2, 1.0f);
		if (!wasCalled) { throw new Exception("Unit test failure - event handler appears to have not been called."); }
 
		// No listener should exist for this event, but we don't require a listener so it should pass
		Messenger<float>.Broadcast(eventType2 + "_", 1.0f, MessengerMode.DONT_REQUIRE_LISTENER);
 
		try {
			// Broadcasting for an event there exists listeners for, but using wrong signature
			Messenger<float>.Broadcast(eventType1, 1.0f, MessengerMode.DONT_REQUIRE_LISTENER);
			throw new Exception("Unit test failure - expected a BroadcastException");
		}
		catch (MessengerInternal.BroadcastException e) {
			// All good
		}
 
		try {
			// Same thing, but now we (implicitly) require at least one listener
			Messenger<float>.Broadcast(eventType2 + "_", 1.0f);
			throw new Exception("Unit test failure - expected a BroadcastException");
		} catch (MessengerInternal.BroadcastException e) {
			// All good
		}
 
		try {
			// Wrong generic type for this broadcast, and we implicitly require a listener
			Messenger<double>.Broadcast(eventType2, 1.0);
			throw new Exception("Unit test failure - expected a BroadcastException");
		} catch (MessengerInternal.BroadcastException e) {
			// All good
		}
 
	}
 
 
	private void RunRemoveTests() {
 
		try {
			// Removal with wrong signature should fail
			Messenger<float>.RemoveListener(eventType1, TestCallbackFloat);
			throw new Exception("Unit test failure - expected a ListenerException");
		}
		catch (MessengerInternal.ListenerException e) {
			// All good
		}
 
		Messenger.RemoveListener(eventType1, TestCallback);
 
		try {
			// Repeated removal should fail
			Messenger.RemoveListener(eventType1, TestCallback);
			throw new Exception("Unit test failure - expected a ListenerException");
		}
		catch (MessengerInternal.ListenerException e) {
			// All good
		}
 
 
 
		Messenger<float>.RemoveListener(eventType2, TestCallbackFloat);
 
		try {
			// Repeated removal should fail
			Messenger<float>.RemoveListener(eventType2, TestCallbackFloat);
			throw new Exception("Unit test failure - expected a ListenerException");
		}
		catch (MessengerInternal.ListenerException e) {
			// All good
		}
	}
 
 
	void TestCallback() {
		wasCalled = true;
		Console.Out.WriteLine("TestCallback() was called.");
	}
 
	void TestCallbackFloat(float f) {
		wasCalled = true;
		Console.Out.WriteLine("TestCallbackFloat(float) was called.");
 
		if (f != 1.0f) {
			throw new Exception("Unit test failure - wrong value on float argument");
		}
	}
 
 
 
}