import asyncio
import tornado


class UploadHandler(tornado.web.RequestHandler):
    def post(self):
        try:
            query = self.get_body_argument("query")
            devices = rank(query)

            if len(devices) == 0:
                self.set_status(404)
            else:
                self.set_status(200)
                self.write(devices)
        except e:
            self.set_status(500)
            self.write(f"Internal error: {e}")

def make_app():
    return tornado.web.Application([
        tornado.web.url(r"/query", QueryHandler),
    ])

async def main():
    app = make_app()
    app.listen(config.PORT)
    await asyncio.Event().wait()

if __name__ == "__main__":
    asyncio.run(main())
