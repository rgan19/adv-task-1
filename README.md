# Adv-Task 1



- Ensure that there is a rabbitmq server running with queue named `tasks` for this task-producer deployment. 
- After running `producer-deploy.yaml`,  type this into url: http://localhost:31500/swagger/index.html 

- POST Request in swagger will create task in rabbitmq `tasks` queue

Sample Request
```
{
    "taskID": 2,
    "customerID": 120,
    "description": "Task 2",
    "priority": "Low",
    "status": 1
}
```

Sample Response:
```
{
  "taskID": 2,
  "customerID": 120,
  "description": "Task 2",
  "priority": "Low",
  "status": "STARTED"
}
```

- 4 status numbers

| number | status     |
|---|---------------|
| 0 | "STARTED"     |
| 1 | "IN_PROGRESS" |
| 2 | "COMPLETED"   |
| 3 | "FAILED"      |

