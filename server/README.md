# Accept a Payment with Stripe Checkout

Stripe Checkout is the fastest way to get started with payments. Included are some basic build and run scripts you can use to start up the application.

## Set Price ID

In the back end code, replace `{{PRICE_ID}}` with a Price ID (`price_xxx`) that you created.

## Running the sample

1. Build the server

~~~
dotnet restore
~~~

2. Run the server

~~~
dotnet run
~~~

3. Go to [http://localhost:4242/checkout.html](http://localhost:4242/checkout.html)