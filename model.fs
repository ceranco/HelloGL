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

// uniform Material material;
uniform TexturedMaterial material;
uniform FlashLight light;
// uniform PointLight light;
// uniform DirectionalLight light;
uniform vec3 viewPos;
// uniform float time;

void main() {
  vec3 diffuseColor = vec3(texture(material.diffuse, TexCoords));
  vec3 specularColor = vec3(texture(material.specular, TexCoords));

  // ambient
  vec3 ambient = light.ambient * diffuseColor;

  // diffuse
  vec3 norm = normalize(Normal);
  //   vec3 lightDir = normalize(-light.direction);
  vec3 lightDir = normalize(light.position - FragPos);
  float diff = max(dot(norm, lightDir), 0.0);
  vec3 diffuse = light.diffuse * diff * diffuseColor;

  // specular
  vec3 viewDir = normalize(viewPos - FragPos);
  vec3 reflectDir = reflect(-lightDir, norm);
  float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
  vec3 specular = light.specular * spec * specularColor;

  // attenuation
  float distance = length(light.position - FragPos);
  float attenuation = 1.0 / (light.constant + light.linear * distance +
                             light.quadratic * (distance * distance));

  float theta = dot(lightDir, -light.direction);
  float epsilon = light.cutOff - light.outerCutOff;
  float intensitiy = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);

  vec3 result = mix((ambient + (diffuse + specular) * intensitiy), ambient,
                    step(theta, light.outerCutOff)) *
                attenuation;
  FragColor = vec4(result, 1.0);
}