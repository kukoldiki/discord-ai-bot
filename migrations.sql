CREATE TABLE user_settings (
    user_id BIGINT NOT NULL PRIMARY KEY,
    model TEXT NOT NULL,
    system_prompt TEXT NOT NULL
);