#!/bin/sh
set -eu

ROOT_PATH="${1:-$PWD}"
PUBLISH_PROFILE_FILE="${2:-registry-prod.pubxml}"

# Returns path relative to ROOT_PATH and with forward slashes.
relative_path() {
  full_path="$1"
  root_path="$2"

  full_path="$(cd "$(dirname "$full_path")" && pwd)/$(basename "$full_path")"
  root_path="$(cd "$root_path" && pwd)"

  case "$full_path" in
    "$root_path"/*)
      printf '%s\n' "${full_path#"$root_path"/}"
      ;;
    *)
      printf '%s\n' "$full_path"
      ;;
  esac
}

# Reads first occurrence of XML property value from a file.
read_xml_property() {
  file_path="$1"
  property_name="$2"

  if [ ! -f "$file_path" ]; then
    echo "File not found: $file_path" >&2
    return 1
  fi

  value="$(sed -n "s:.*<${property_name}>\([^<]*\)</${property_name}>.*:\1:p" "$file_path" | head -n 1)"

  if [ -z "$(printf '%s' "$value" | tr -d '[:space:]')" ]; then
    echo "PropertyGroup/$property_name value not found in file: $file_path" >&2
    return 1
  fi

  printf '%s\n' "$value"
}

script_dir="$(cd "$ROOT_PATH" && pwd)"
applications_path="$script_dir/Source/Applications"

if [ ! -d "$applications_path" ]; then
  echo "Directory not found: $applications_path" >&2
  exit 1
fi

tmp_project_dirs_file="$(mktemp)"
trap 'rm -f "$tmp_project_dirs_file"' EXIT HUP INT TERM

find "$applications_path" -type f -name "RepositorySettings.props" | while IFS= read -r filepath; do
  project_dir="$(dirname "$filepath")"
  if [ -f "$project_dir/jenkins-ignore" ]; then
    continue
  fi
  printf '%s\n' "$project_dir"
done | sort -u > "$tmp_project_dirs_file"

if [ ! -s "$tmp_project_dirs_file" ]; then
  echo "No projects found with publish profile $PUBLISH_PROFILE_FILE" >&2
  exit 1
fi

first=1
printf '['

while IFS= read -r project_dir; do
  set -- "$project_dir"/*.csproj
  if [ "$1" = "$project_dir/*.csproj" ]; then
    echo "No .csproj found in directory: $project_dir" >&2
    exit 1
  fi
  project_file="$1"

  repository_settings_path="$project_dir/RepositorySettings.props"
  publish_profile_path="$project_dir/Properties/PublishProfiles/$PUBLISH_PROFILE_FILE"

  container_repository="$(read_xml_property "$repository_settings_path" "ContainerRepository")"
  publish_image_tag="$(read_xml_property "$publish_profile_path" "PublishImageTag")"
  project_path="$(relative_path "$project_file" "$script_dir")"

  escaped_project_path="$(printf '%s' "$project_path" | sed 's/\\/\\\\/g; s/"/\\"/g')"
  escaped_container_repository="$(printf '%s' "$container_repository" | sed 's/\\/\\\\/g; s/"/\\"/g')"
  escaped_publish_image_tag="$(printf '%s' "$publish_image_tag" | sed 's/\\/\\\\/g; s/"/\\"/g')"

  if [ "$first" -eq 0 ]; then
    printf ','
  fi
  first=0

  printf '{"projectPath":"%s","containerRepository":"%s","publishImageTag":"%s"}' \
    "$escaped_project_path" "$escaped_container_repository" "$escaped_publish_image_tag"
done < "$tmp_project_dirs_file"

printf ']\n'
