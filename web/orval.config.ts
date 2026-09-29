import { defineConfig } from "orval";

export default defineConfig({
  reservae: {
    input: {
      target:
        process.env.OPENAPI_URL ??
        "http://localhost:5144/swagger/v1/swagger.json",
    },
    output: {
      mode: "tags-split",
      target: "./lib/api/endpoints.ts",
      schemas: "./lib/api/models",
      client: "fetch",
      clean: true,
      indexFiles: true,
      tagsSplitDeduplication: true,
      baseUrl: {
        runtime:
          'process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5144"',
      },
      override: {
        fetch: {
          includeHttpResponseReturnType: false,
          forceSuccessResponse: true,
        },
      },
    },
  },
});
