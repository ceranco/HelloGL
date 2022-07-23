#version 330 core

struct TexturedMaterial {
  sampler2D diffuse;
  sampler2D specular;
  sampler2D emission;
  float shininess;
};

// struct Material {
//   vec3 ambient;
//   vec3 diffuse;
//   vec3 specular;
//   float shininess;
// };

struct Light {
  vec3 position;

  vec3 ambient;
  vec3 diffuse;
  vec3 specular;
};

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoords;

out vec4 FragColor;

// uniform Material material;
uniform TexturedMaterial material;
uniform Light light;
uniform vec3 viewPos;
uniform float time;

void main() {
  vec3 diffuseColor = vec3(texture(material.diffuse, TexCoords));
  vec3 specularColor = vec3(texture(material.specular, TexCoords));
  vec3 emissionColor = vec3(texture(
      material.emission, mod(TexCoords + vec2(0, 0.5 * time), vec2(1.0))));

  // ambient
  vec3 ambient = light.ambient * diffuseColor;

  // diffuse
  vec3 norm = normalize(Normal);
  vec3 lightDir = normalize(light.position - FragPos);
  float diff = max(dot(norm, lightDir), 0.0);
  vec3 diffuse = light.diffuse * diff * diffuseColor;

  // specular
  vec3 viewDir = normalize(viewPos - FragPos);
  vec3 reflectDir = reflect(-lightDir, norm);
  float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
  vec3 specular = light.specular * spec * specularColor;

  // emission
  vec3 emission = (vec3(1.0) - ceil(specularColor)) * emissionColor;

  vec3 result = ambient + diffuse + specular + emission;
  FragColor = vec4(result, 1.0);
}