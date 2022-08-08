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

struct PointLight {
  vec3 position;

  vec3 ambient;
  vec3 diffuse;
  vec3 specular;

  float constant;
  float linear;
  float quadratic;
};

struct FlashLight {
  vec3 position;
  vec3 direction;
  float cutOff;
  float outerCutOff;

  vec3 ambient;
  vec3 diffuse;
  vec3 specular;

  float constant;
  float linear;
  float quadratic;
};

struct DirectionalLight {
  vec3 direction;

  vec3 ambient;
  vec3 diffuse;
  vec3 specular;
};

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoords;

out vec4 FragColor;

uniform TexturedMaterial material;
uniform vec3 viewPos;
uniform DirectionalLight directionalLight;
#define NUM_POINT_LIGHTS 4
uniform PointLight pointLights[NUM_POINT_LIGHTS];

vec3 CalculateDirectionalLight(DirectionalLight light, vec3 normal,
                               vec3 viewDirection, TexturedMaterial material);
vec3 CalculatePointLight(PointLight light, vec3 normal, vec3 fragPosition,
                         vec3 viewDirection, TexturedMaterial material);

void main() {
  vec3 normal = normalize(Normal);
  vec3 viewDirection = normalize(viewPos - FragPos);

  // Directional lighting
  vec3 result = CalculateDirectionalLight(directionalLight, normal,
                                          viewDirection, material);

  // Point lights
  for (int i = 0; i < NUM_POINT_LIGHTS; i++) {
    result += CalculatePointLight(pointLights[i], normal, FragPos,
                                  viewDirection, material);
  }

  FragColor = vec4(result, 1.0);
}

vec3 CalculateDirectionalLight(DirectionalLight light, vec3 normal,
                               vec3 viewDirection, TexturedMaterial material) {
  vec3 lightDirection = normalize(-light.direction);

  vec3 diffuseColor = vec3(texture(material.diffuse, TexCoords));
  vec3 specularColor = vec3(texture(material.specular, TexCoords));

  // ambient
  vec3 ambient = light.ambient * diffuseColor;

  // diffuse
  float diff = max(dot(normal, lightDirection), 0.0);
  vec3 diffuse = light.diffuse * diff * diffuseColor;

  // specular
  vec3 reflectDirection = reflect(-lightDirection, normal);
  float spec =
      pow(max(dot(viewDirection, reflectDirection), 0.0), material.shininess);
  vec3 specular = light.specular * spec * specularColor;

  return ambient + diffuse + specular;
}

vec3 CalculatePointLight(PointLight light, vec3 normal, vec3 fragPosition,
                         vec3 viewDirection, TexturedMaterial material) {
  vec3 lightDirection = normalize(light.position - fragPosition);

  vec3 diffuseColor = vec3(texture(material.diffuse, TexCoords));
  vec3 specularColor = vec3(texture(material.specular, TexCoords));

  // ambient
  vec3 ambient = light.ambient * diffuseColor;

  // diffuse
  float diff = max(dot(normal, lightDirection), 0.0);
  vec3 diffuse = light.diffuse * diff * diffuseColor;

  // specular
  vec3 reflectDirection = reflect(-lightDirection, normal);
  float spec =
      pow(max(dot(viewDirection, reflectDirection), 0.0), material.shininess);
  vec3 specular = light.specular * spec * specularColor;

  // attenuation
  float distance = length(light.position - fragPosition);
  float attenuation = 1.0 / (light.constant + light.linear * distance +
                             light.quadratic * distance * distance);

  return (ambient + diffuse + specular) * attenuation;
}