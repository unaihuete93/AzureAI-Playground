
| Term       | Definition |
|------------|------------|
| Assistant  | Custom AI that uses Azure OpenAI models in conjunction with tools. |
| Thread     | A conversation session between an Assistant and a user. Threads store Messages and automatically handle truncation to fit content into a model’s context. |
| Message    | A message created by an Assistant or a user. Messages can include text, images, and other files. Messages are stored as a list on the Thread. |
| Run        | Activation of an Assistant to begin running based on the contents of the Thread. The Assistant uses its configuration and the Thread’s Messages to perform tasks by calling models and tools. As part of a Run, the Assistant appends Messages to the Thread. |
| Run Step   | A detailed list of steps the Assistant took as part of a Run. An Assistant can call tools or create Messages during it’s run. Examining Run Steps allows you to understand how the Assistant is getting to its final results. |