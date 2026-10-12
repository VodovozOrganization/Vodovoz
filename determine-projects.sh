#!/bin/sh
set -eu

ROOT_PATH="${1:-$PWD}"
PUBLISH_PROFILE_FILE="${2:-registry-prod.pubxml}"

# Base profile name (registry-prod), used to find organization profiles (registry-prod-<organization>.pubxml).
PUBLISH_PROFILE_NAME="${PUBLISH_PROFILE_FILE%.pubxml}"

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

json_escape() {
  printf '%s' "$1" | sed 's/\\/\\\\/g; s/"/\\"/g'
}

# Prints publishing entry: project path, container repository, image tag.
print_project() {
  if [ "$first" -eq 0 ]; then
    printf ','
  fi
  first=0
  project_images_count=$((project_images_count + 1))

  printf '{"projectPath":"%s","containerRepository":"%s","publishImageTag":"%s"}' \
    "$(json_escape "$1")" "$(json_escape "$2")" "$(json_escape "$3")"
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
  publish_profiles_dir="$project_dir/Properties/PublishProfiles"
  publish_profile_path="$publish_profiles_dir/$PUBLISH_PROFILE_FILE"
  project_path="$(relative_path "$project_file" "$script_dir")"
  project_images_count=0

  # Main image: repository from RepositorySettings.props, tag from the main profile.
  if [ -f "$publish_profile_path" ]; then
    container_repository="$(read_xml_property "$repository_settings_path" "ContainerRepository")"
    publish_image_tag="$(read_xml_property "$publish_profile_path" "PublishImageTag")"
    print_project "$project_path" "$container_repository" "$publish_image_tag"
  fi

  # Organization images: each <profile>-<organization>.pubxml defines its own repository and tag.
  # The project is published as a separate entry for each of these repositories.
  for organization_profile_path in "$publish_profiles_dir/$PUBLISH_PROFILE_NAME"-*.pubxml; do
    if [ -f "$organization_profile_path" ]; then
      container_repository="$(read_xml_property "$organization_profile_path" "ContainerRepository")"
      publish_image_tag="$(read_xml_property "$organization_profile_path" "PublishImageTag")"
      print_project "$project_path" "$container_repository" "$publish_image_tag"
    fi
  done

  if [ "$project_images_count" -eq 0 ]; then
    echo "No publish profiles $PUBLISH_PROFILE_FILE or $PUBLISH_PROFILE_NAME-*.pubxml found for project: $project_path" >&2
    exit 1
  fi
done < "$tmp_project_dirs_file"

printf ']\n'
