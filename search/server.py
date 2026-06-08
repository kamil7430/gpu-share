import asyncio
import tornado
import tornado.escape
from rank import rank


class HealthHandler(tornado.web.RequestHandler):
    def get(self):
        self.set_status(200)

class QueryHandler(tornado.web.RequestHandler):
    def post(self):
        try:
            payload = tornado.escape.json_decode(self.request.body)
            query = payload["query"]
            devices = rank(query)

            if len(devices) == 0:
                self.set_status(404)
            else:
                self.set_status(200)
                self.write(str(list(map(lambda d: d['id'], devices))))
        except Exception as e:
            self.set_status(500)
            self.write(f"Internal error: {e}")


def make_app():
    return tornado.web.Application([
        tornado.web.url(r"/query", QueryHandler),
        tornado.web.url(r"/health", HealthHandler),
    ])

async def main():
    port = 2140
    app = make_app()
    app.listen(port)
    print(f"Search service running on {port}")
    await asyncio.Event().wait()

if __name__ == "__main__":
    asyncio.run(main())
