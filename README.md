How to run
- Must have docker running
1.  run 'dotnet ef database update --project VinApplication --startup-project VinApplication' from the solution folder in a terminal / cmd prompt
2. inside terminal of docker 'docker-compose build'
3. 'docker-compose up -d db' to start db
4. run migrations with 'docker-compose run web dotnet ef database update'


Finally start everything in docker with 'docker-compose-up'

With a ton more time, I'd add an entrypoint.sh which would do this everytime at startup.
