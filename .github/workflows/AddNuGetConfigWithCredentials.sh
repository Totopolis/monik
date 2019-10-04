#!/bin/bash
sed -r "s|GRPSOURCE|$GRPSOURCE|i" ./.github/workflows/NuGet.config \
    | sed -r "s|GRPUSERNAME|$GRPUSERNAME|i" \
    | sed -r "s|GRPPASS|$GRPPASS|i" > ./NuGet.config
