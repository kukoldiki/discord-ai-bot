CREATE TABLE user_settings
(
    user_id       BIGINT             NOT NULL PRIMARY KEY,
    model         TEXT               NOT NULL,
    system_prompt TEXT               NOT NULL,
    thinking      BOOL DEFAULT false NOT NULL
);

CREATE
EXTENSION IF NOT EXISTS vector;

CREATE TABLE memories
(
    id         BIGSERIAL PRIMARY KEY,
    user_id    BIGINT NOT NULL,
    content    TEXT NOT NULL,
    embedding  vector(768) NOT NULL, -- nomic-embed-text
    created_at TIMESTAMP DEFAULT NOW(),
    type       TEXT      DEFAULT 'fact'
);

CREATE INDEX memories_embedding_idx
    ON memories
    USING hnsw (embedding vector_cosine_ops);