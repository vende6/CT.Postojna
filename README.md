# CT.Postojna

go to C:\Users\CT\source\repos\CT.Postojna

and open CT.Postajna.slnx

Run the app by clicking "https" at the top of Visual Studio program.

GitHub Pages serves static files and cannot host this interactive ASP.NET Core app. A Render Blueprint is provided in `render.yaml`; create a new Blueprint from this repository in Render to provision the web service.

Configure these environment variables as secrets in the Render dashboard (never commit their values):

- `ConnectionStrings__DefaultConnection`: the rotated MySQL connection string. The database must allow connections from Render.
- `Stripe__SecretKey`: a Stripe test key for testing or a live key for production.

The free Render service sleeps when idle and may take about a minute to wake. Render's free tier is intended for testing and hobby projects, not production. Local `appsettings*.json` files are excluded from the Docker build and publish output. Rotate database and demo-account credentials that were previously committed; removing them from the working tree does not remove them from Git history.
