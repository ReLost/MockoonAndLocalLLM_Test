# Project Overview

This project was built using **Unity 6000.2.14f1**.  
It provides a complete workflow for handling web requests, parsing incoming data, controlling UI elements, and managing character animations based on data received from WebRequests.

---

## Table of Contents

- [Unity Version](#unity-version)
- [Web Request Handling — `NetworkHandler`](#web-request-handling--networkhandler)
- [Data Parsing — `DataParser`](#data-parsing--dataparser)
- [User Interface — `ConversationUI` and `CharacterUI`](#user-interface--conversationui-and-characterui)
- [Character Animation — `CharacterAnimatorController`](#character-animation--characteranimatorcontroller)

---

## Unity Version

This project was created and tested with:

**Unity 6000.2.14f1**

For best compatibility, use the same or a newer 6000.x version.

---

## Web Request Handling — `NetworkHandler`

All communication with the server is managed through the **NetworkHandler** script.  
It exposes four events that allow you to hook into different stages of the WebRequest lifecycle:

```csharp
public event Action OnResponseWaiting;
public event Action<string> OnResponseReceivedSuccess;
public event Action OnResponseReceivedFailure;
public event Action OnResponseTimeout;
```

### Additional Features

- **The timeout duration can be configured directly from the Unity Inspector**, allowing easy adjustments without modifying code.

### Event Descriptions

- **OnResponseWaiting**  
  Triggered while waiting for a response.

- **OnResponseReceivedSuccess(string)**  
  Triggered when a response is received successfully.  
  Returns the WebRequest result as a **string**.

- **OnResponseReceivedFailure**  
  Triggered when a response fails.

- **OnResponseTimeout**  
  Triggered if the response exceeds the expected timeout duration.

---

## Data Parsing — `DataParser`

Incoming WebRequest data can be converted into structured, readable formats using the **DataParser**.

It currently supports parsing into:

- `ResponseData`
- `CharacterData`
- `Status` (as a `string`)

---

## User Interface — `ConversationUI` and `CharacterUI`

### `ConversationUI`

Responsible for the chat-related UI:
- Input Field for entering messages  
- Text Field showing the full message history  
- Send Button  
- Ability to send messages via keyboard shortcut  
- Easily extendable to support additional input devices, **including new inputs from other external devices**  

### `CharacterUI`

Currently manages the character satisfaction bar:
- Its color reflects the current satisfaction value

---

## Character Animation — `CharacterAnimatorController`

The **CharacterAnimatorController** updates character animations based on the status received from the WebRequest.

Available animations are defined in a **ScriptableObject** called `CharacterStatusData`.

- If a received status is **not** listed in the ScriptableObject, a **default status** defined there will be used.

---
