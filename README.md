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
- [Data Structures](#data-structures)
  - [WebRequest Data](#webrequest-data)
  - [Character Data](#character-data)
- [Adding New Features](#adding-new-features)
  - [Adding a New Character Animation](#adding-a-new-character-animation)
  - [Handling a New URL Endpoint](#handling-a-new-url-endpoint)
-  [Text-to-Speech (TTS)](#text-to-speech-tts)
-  [Roadmap](#roadmap)
---

## Unity Version

This project was created and tested with:

**Unity 6000.2.14f1**

For best compatibility, use the same or a newer 6000.x version.

---

## Web Request Handling — `NetworkHandler`

<img width="816" height="141" alt="Zrzut ekranu 2025-12-04 004651" src="https://github.com/user-attachments/assets/a0d08021-b8ea-4a5e-a2dc-8796b648ad66" />

All communication with the server is managed through the **NetworkHandler** script.  
It exposes four events that allow you to hook into different stages of the WebRequest lifecycle:

```csharp
public event Action OnResponseWaiting;
public event Action<string> OnResponseReceivedSuccess;
public event Action<string> OnResponseReceivedFailure;
public event Action OnResponseTimeout;
```

### Features

- **Support for sending requests to a local Ollama instance**, enabling local AI responses.  
- **The timeout duration can be configured directly from the Unity Inspector**, allowing easy adjustments without modifying code.  

### Event Descriptions

- **OnResponseWaiting**  
  Triggered while waiting for a response.

- **OnResponseReceivedSuccess(string)**  
  Triggered when a response is received successfully.  
  Returns the WebRequest result as a **string**.

- **OnResponseReceivedFailure(string)**  
  Triggered when a response fails.  
  Returns the error message as a **string**.

- **OnResponseTimeout**  
  Triggered if the response exceeds the expected timeout duration.

---

## Data Parsing — `DataParser`

Incoming WebRequest data can be parsed into readable structures.  
**Supported data types:**  

- `ResponseData`  
- `CharacterData`  
- `Status` (string)  
- `OllamaResponseData` — represents a single chunk of data returned by Ollama. Multiple chunks can be combined to obtain the complete response as a **string**.

---

## User Interface — `ConversationUI` and `CharacterUI`

### `ConversationUI`

Responsible for the chat-related UI:
- Input Field for entering messages  
- Text Field showing the full message history  
- Send Button  
- Ability to send messages via keyboard shortcut  
- Easily extendable to support additional input devices, **including new inputs from other external devices**  
- **Device input can be configured to send messages either to Ollama or to the local host bot**, allowing flexible routing of user input.
- Optional **Text-to-Speech support**, allowing received responses to be spoken aloud
<img width="842" height="416" alt="Zrzut ekranu 2025-12-04 050410" src="https://github.com/user-attachments/assets/f4163f41-2fe8-4be1-83e4-5dc7c2d34885" />

### `CharacterUI`

Currently manages the character satisfaction bar:
- Its color reflects the current satisfaction value
<img width="523" height="936" alt="Zrzut ekranu 2025-12-04 050316" src="https://github.com/user-attachments/assets/63bacb61-d3ca-4757-9c0a-1bcab78130cd" />

---

## Character Animation — `CharacterAnimatorController`

The **CharacterAnimatorController** updates character animations based on the status received from the WebRequest.

Available animations are defined in a **ScriptableObject** called `CharacterStatusData`.

- If a received status is **not** listed in the ScriptableObject, a **default status** defined there will be used.
  <img width="816" height="222" alt="Zrzut ekranu 2025-12-03 212248" src="https://github.com/user-attachments/assets/6afc401b-13d2-4e25-998a-dc6ba13c5cab" />

---

## Data Structures

This project uses several data structures to manage incoming and outgoing data for WebRequests, characters, and animations.

---

### WebRequest Data

These structures are used for sending requests and parsing responses from WebRequests and Ollama.

#### `ResponseData`
- **Purpose:** Base structure obtained from a WebRequest.
- **Fields:**
  - `character` (**CharacterData**) — contains character-related information.

#### `CharacterData`
- **Purpose:** Holds information about a character.
- **Fields:**
  - `name` (**string**) — character's name.
  - `satisfaction` (**int**) — character's satisfaction level.
  - `mood` (**int**) — character's current mood.

#### `OllamaRequestData`
- **Purpose:** Represents the data sent along with a prompt to Ollama.
- **Fields:**
  - `model` (**string**) — the Ollama model to use.
  - `prompt` (**string**) — the text prompt to send to Ollama.

#### `OllamaResponseData`
- **Purpose:** Represents a single chunk of data returned by Ollama.  
  Multiple chunks can be combined to obtain the complete response as a string.
- **Fields:**
  - `response` (**string**) — a single chunk of the response from Ollama.
  - `done` (**bool**) — indicates whether this chunk is the last; when `true`, all received chunks together form the complete response.

---

### Character Data

These structures manage character-related information and animation states.

#### `CharacterStatusData`
- **Purpose:** Holds available animations for a character and defines the default status to use if a received status is missing.
- **Fields:**
  - `availableCharacterStatuses` (**`List<string>`**) — a list of statuses representing available animations for the character.
  - `defaultCharacterStatus` (**string**) — the default status to use when a received status is not found in `availableCharacterStatuses`.

---

## Adding New Features

This section explains how to extend the project by adding new character animations and handling new URL endpoints.

---

### Adding a New Character Animation

To add a new animation for a character:

1. **Update the Animator Controller**
   - Open the **HumanoidBaseAnimationController** Animator.
   - Add the new animation clip.
   - Create transitions to and from existing animations, following the pattern of existing transitions.
   - Add a new **Trigger parameter** that corresponds to the new character status.
     <img width="744" height="347" alt="Zrzut ekranu 2025-12-04 010401" src="https://github.com/user-attachments/assets/af2e9a4d-cfeb-4c5d-a9f9-af96bb005cf7" />


2. **Update `CharacterStatusData`**
   - Add the new status string to the **`availableCharacterStatuses`** list.
   - Optionally, adjust the **`defaultCharacterStatus`** if you want this new status to be the default in certain cases.
     <img width="816" height="222" alt="Zrzut ekranu 2025-12-03 212248" src="https://github.com/user-attachments/assets/ac4cfb23-356f-42a7-b290-2d8738280a21" />


---

### Handling a New URL Endpoint

To handle a new server endpoint:

1. **Create a new ScriptableObject for the `NetworkHandler`**
   - Set the endpoint URL in the ScriptableObject.
   - For basic GET/POST requests, it should work without further modifications.
   - Ensure the event handlers (`OnResponseReceivedSuccess`, `OnResponseReceivedFailure`, etc.) are subscribed in your scripts to handle the responses.
     <img width="901" height="276" alt="Zrzut ekranu 2025-12-04 010457" src="https://github.com/user-attachments/assets/e586858f-6cb5-4759-8dea-a4978b342649" />
     <img width="817" height="141" alt="Zrzut ekranu 2025-12-04 010527" src="https://github.com/user-attachments/assets/8bf68367-18f3-493d-a8f9-881cb1beec3e" />

---

## Text-to-Speech (TTS)

This project includes **Text-to-Speech functionality** for reading aloud received responses from the chat.  

- The TTS implementation is based on the model: [Jets Text-to-Speech](https://huggingface.co/unity/inference-engine-jets-text-to-speech) from Hugging Face.  
- It has been integrated into the project with minor modifications to fit the existing Unity workflow and UI.  
- TTS can be enabled via the **ConversationUI**, allowing messages received from either Ollama or the local host bot to be spoken aloud.  

---

## Roadmap

We plan to introduce the following features in future versions of the project:

- **Speech-to-Text (STT)** – ability to recognize user speech and send it as text to the system.  
- **Ollama model selection** – allowing the user to choose the AI model when sending prompts, providing more flexibility in generating responses.

