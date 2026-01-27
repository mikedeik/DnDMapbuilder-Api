- Run the following command on the repo top level: ```powershell   repomix --output repomix-output.xml --include "**/*.cs,**/*.csproj,**/*.json,**/*.yml,**/*.yaml,**/*.md,**/*.vue,**/*.js,**/*.ts,**/*.html,**/*.css,**/*.scss" --ignore "**/bin/**,**/obj/**,**/.vs/**,**/packages/**,**/.nuget/**,**/node_modules/**,**/dist/**,**/.vite/**" --style xml ` --remove-comments --remove-empty-lines ```

