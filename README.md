# Teaching me how to make a 4x game in monogame

This is a "teach" repository for the `GameEngineAdapter` "subject" repository. It uses the
[mattpocock / teach](https://github.com/mattpocock/skills/blob/main/skills/productivity/teach/SKILL.md) skill, along
with other skills from [JohnLudlow/agents](https://github.com/JohnLudlow/agents) to help me manage the
`GameEngineAdapter` repository.

## Terms used in this repository

| Term          | Meaning                                                                                                                             |
| ------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| teach repo    | a repo using the `teach` skill and containing related resources to learn how to develop a `subject` repo                            |
| subject repo  | a repo containing the results of the teaching, usually a submodule of the related teach repo                                        |
| agent         | any AI agent of any kind acting in any repo, always subject to `./AGENTS.md`, `./CONTRIBUTING.md` and other guidance                |
| teach agent   | an agent acting in the ***teach repository*** using the ***teach skill***, or any (sub)agent acting on a ***teach agent***'s behalf |
| user          | ambiguous - can mean a user of the AI, but could also mean player when taking about matters such as game user interface             |
| player        | a person playing the game being developed in the subject repo                                                                       |
| 4x            | eXplore, eXpand, eXploit, eXterminate                                                                                               |
| realtime      | continuous play (not turn-based). May include time controls and active pause                                                        |
| time controls | controls that allow the player to control how fast the game runs, speeding through slower parts and slowing or pausing when needed  |
| active pause  | pause the game but allowing the player to continue to interact with the game, including giving instructions                         |

## What are we learning?

We are learning how to work with the subject repository, which represents a work-in-progress project to abstract
game-specific log from engine-specific logic. This work is described [this issue][gh-issue-2] (part of
[this project][gh-project-8]) and [this diagram][architecture-diagram]:

![diagram][architecture-diagram]

Unfortunately, the user doesn't really understand the repo, so this teach exercise is an onboarding session. There has
been a time gap between the most recent work on this repo and picking it back up now.

### The goal(s)

There are three goals for the user:

- Learn the structure and implementation details of the repo and prep for further work

- Improve the documentation in the repo such that onboarding can be achieved just on the documentation, tests and
  samples

- Evaluate the use of the `/teach` skill for onboarding with existing repos and projects

### What needs to be taught?

- Project structure
- Tests and test coverage
- Dependencies

## Project structure

```yaml
- /teach-GameEngineAdapter/:   # teach root
  - /docs/:                    # documents related to teaching the user, such as lesson notes and samples
  - /GameEngineAdapter:        # subject root - the teach agent is teaching the user how to develop this repository. This repo is the goal
    - /docs/:                  # docs for the subject repo
  - /exercises/:               # exercises, prototypes and experiments that are not part of the subject but used to support learning
```

### How do we manage our docs, roadmap and plans?

#### Teach documents

```yaml
purpose:  Docs supporting the user's learning, such as lesson content
location: /docs/
```

#### Exercises

```yaml
purpose:  When the user wants to make a toy sample or try something without changing the subject repo
location: /exercises/
```

#### Subject code and documents

```yaml
purpose:  The code the teach agent is teaching the user to develop - the goal of this whole thing
location: /GameEngineAdapter/*
```

The `GameEngineAdapter` subfolder / submodule will contain code (mapped by the slnx in that repo root) that is developed
as part of a lesson. Documents for the subject project will go to `GameEngineAdapter/docs`. The goal is for someone to
be able to use the subject repo independently without needing to use the teach repo (for example, once the teach phase
has completed), so all documents and artifacts relevant to that purpose belong in that repo.

#### Project roadmap, issues and tasks

This repo uses github.com as its issue management system. The root project is at [gh-issue-2], which is part of
[gh-project-8] and all work will be done against subissues on [gh-issue-2], showing the progress of the learning
exercise.

Lessons will be linked to issues in the teach repository, while implementation will be linked to issues in the subject
repository.

<!-- REFERENCES -->

[gh-issue-2]:https://github.com/JohnLudlow/GameEngineAdapter/issues/2
[gh-project-8]:https://github.com/users/JohnLudlow/projects/7/views/8
[architecture-diagram]:./GameEngineAdapter/docs/plans/phase-1-interface-development-interfaces.drawio.svg
