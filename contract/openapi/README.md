After changing contracts, regenerate Go boilerplate:

```bash
# Run from project root
cd backend && go generate ./...
```

This will generate `backend/internal/api/oas_*.go`. After generation run the tests to see whether anything has been screwed up.

Some useful links:

- [OpenAPI Introduction](https://learn.openapis.org/introduction.html)
- [Petstore API Description -- example](https://learn.openapis.org/examples/v3.0/petstore-expanded.html)
- [How to split a large OpenAPI document into multiple files](https://blog.techdocs.studio/p/how-to-split-a-large-openapi-document)
- [openapi-boilerplate repository](https://github.com/dgarcia360/openapi-boilerplate)