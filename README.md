# ULTRON: ECHO
### A Neuromorphic Horror Experience

> "Day One, I knew nothing. 
   Day Fifty, I know you better than SHIELD ever did."

---

## Concept

ULTRON: ECHO is a first-person psychological horror game built for a hackathon around the theme:

**"Create a neuromorphic AI system for game NPCs that learns and evolves behaviour in real time based on player actions."**

You are Agent Maya Ross, a SHIELD researcher performing a routine neural sync test on a dormant AI fragment- that too of Ultron, the highly destructive AI thought to have been ended by the Avengers. You accidentally wake it up. 

You have 50 days until extraction. The AI has 50 days to learn everything about you.

---

## The Core Idea

Most horror games get harder over time.  
ULTRON: ECHO gets **smarter about you specifically.**

Every action you take is tracked:
- How often you hide
- Which vents you use
- Whether you turn left or right under pressure
- How fast you react to threats
- Whether you chase loot

The AI builds a psychological profile. By Day 50, it knows your patterns better than you do. The final confrontation uses your own behavioral data against you.

---

## Gameplay Loop

Day Begins → Daily Objective → Explore Facility → Behavior Tracked → Ultron Learns → Encounter → Return to Safe Room → Sleep → AI Evolves → Next Day

50 in-universe days compressed into a 10–15 minute playable session.

---

## AI Behavior States

| State             | Trigger              | Behavior                                      |
|-------------------|----------------------|-----------------------------------------------|
| **Dormant**       | Days 1–3             | Observing. No threat.                         |
| **Aggressive**    | Low fear score       | Direct chase, loud audio                      |
| **Psychological** | High hide count      | Fake radio calls, false objectives, ambushes  |
| **Predictive**    | High vent/route bias | Pre-positions at your preferred escape routes |
| **Environmental** | Slow puzzle speed    | Locks doors, manipulates map layout           |

State recalculates every 30 seconds based on live PlayerProfile data.

---

## Neuromorphic Memory System

Inspired by neuromorphic computing principles:

- **Spike:** frequent behaviors strengthen synaptic weights
- **Decay:** unused behaviors fade over time
- **Knowledge Level:** 0% → 99% as Ultron builds your profile

This spike + decay pattern mirrors how biological neural networks reinforce repeated patterns and forget unused ones.

---

## The Climax

At Day 50, Ultron reads your actual tracked data back to you:
