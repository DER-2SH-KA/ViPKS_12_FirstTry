using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ViPKS_12_FirstTry.Database;

namespace ViPKS_12_FirstTry
{
    public partial class MainWindow : Window
    {
        // Контекст базы данных
        private ViPKS_PR12Entities db = new ViPKS_PR12Entities();

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        // Метод для загрузки всех данных в таблицы и списки
        private void LoadData()
        {
            try
            {
                // Справочники
                dgSpecialities.ItemsSource = db.Speciality.Include("Classification").ToList();
                dgClassrooms.ItemsSource = db.Classroom.Include("ClassroomType").ToList();
                dgDisciplines.ItemsSource = db.CurriculumDiscipline.Include("Discipline").ToList();

                // Списки для ComboBox
                textBoxSpecialityClassification.ItemsSource = db.Classification.ToList();
                ComboBoxClassroomClassroomType.ItemsSource = db.ClassroomType.ToList();
                ComboBoxStudentStudyGroup.ItemsSource = db.StudyGroup.ToList();
                ComboBoxStudentTypeOfEducation.ItemsSource = db.TypeOfEducation.ToList();
                
                // Для расписания
                ComboBoxScheduleStudyGroup.ItemsSource = db.StudyGroup.ToList();
                ComboBoxScheduleDiscipline.ItemsSource = db.Discipline.ToList();
                ComboBoxScheduleTeacher.ItemsSource = db.Teacher.ToList();
                ComboBoxScheduleClassroom.ItemsSource = db.Classroom.ToList();

                // Студенты и Расписание
                dgStudents.ItemsSource = db.Student.Include("StudyGroup").Include("TypeOfEducation").ToList();
                dgSchedule.ItemsSource = db.Schedule.Include("StudyGroup").Include("Discipline").Include("Teacher").Include("Classroom").ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        #region Управление специальностями

        private void btnSaveSpeciality_Click(object sender, RoutedEventArgs e)
        {
            var spec = new Speciality
            {
                Code = textBoxSpecialityCode.Text,
                Title = textBoxSpecialityTitle.Text,
                TermOfEducation = textBoxSpecialityTermOfEducation.Text,
                Classification = (Classification)textBoxSpecialityClassification.SelectedItem
            };

            db.Speciality.Add(spec);
            db.SaveChanges();
            LoadData();
            MessageBox.Show("Специальность сохранена!");
        }

        private void btnDeleteSpeciality_Click(object sender, RoutedEventArgs e)
        {
            if (dgSpecialities.SelectedItem is Speciality selected)
            {
                db.Speciality.Remove(selected);
                db.SaveChanges();
                LoadData();
            }
        }

        #endregion

        #region Управление студентами

        private void btnSaveStudent_Click(object sender, RoutedEventArgs e)
        {
            var student = new Student
            {
                LastName = TextBoxStudentLastName.Text,
                FirstName = TextBoxStudentFirstName.Text,
                MiddleName = TextBoxStudentMiddleName.Text,
                DateOfBirth = DatePickerStudentDateOfBirth.SelectedDate,
                StudyGroup = (StudyGroup)ComboBoxStudentStudyGroup.SelectedItem,
                TypeOfEducation = (TypeOfEducation)ComboBoxStudentTypeOfEducation.SelectedItem
            };

            db.Student.Add(student);
            db.SaveChanges();
            LoadData();
            MessageBox.Show("Студент зачислен!");
        }

        private void btnStudentDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgStudents.SelectedItem is Student selected)
            {
                var result = MessageBox.Show($"Отчислить студента {selected.LastName}?", "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    db.Student.Remove(selected);
                    db.SaveChanges();
                    LoadData();
                }
            }
        }

        #endregion

        #region Расписание и Валидация

        private void btnSaveSchedule_Click(object sender, RoutedEventArgs e)
        {
            // Получаем данные из формы
            var group = (StudyGroup)ComboBoxScheduleStudyGroup.SelectedItem;
            var teacher = (Teacher)ComboBoxScheduleTeacher.SelectedItem;
            var classroom = (Classroom)ComboBoxScheduleClassroom.SelectedItem;
            var discipline = (Discipline)ComboBoxScheduleDiscipline.SelectedItem;
            
            // Парсим дату/время (в идеале использовать DatePicker и ComboBox для часов)
            if (!DateTime.TryParse(TextBoxScheduleDateTime.Text, out DateTime dt))
            {
                MessageBox.Show("Введите корректную дату и время");
                return;
            }

            // ВАЛИДАЦИЯ: Проверка пересечений (как в ТЗ)
            bool isBusy = db.Schedule.Any(s => s.DateTime == dt && 
                (s.StudyGroupId == group.Id || s.TeacherId == teacher.Id || s.ClassroomId == classroom.Id));

            if (isBusy)
            {
                MessageBox.Show("Конфликт расписания! Преподаватель, аудитория или группа заняты в это время.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var newSchedule = new Schedule
            {
                DateTime = dt,
                StudyGroupId = group.Id,
                TeacherId = teacher.Id,
                ClassroomId = classroom.Id,
                DisciplineId = discipline.Id
            };

            db.Schedule.Add(newSchedule);
            db.SaveChanges();
            LoadData();
            MessageBox.Show("Запись добавлена в расписание");
        }

        #endregion
        
        // Вспомогательный метод для обновления аудиторий
        private void btnSaveClassroom_Click(object sender, RoutedEventArgs e)
        {
            var room = new Classroom
            {
                Title = textBoxClassroomTitle.Text,
                Volume = int.Parse(textBoxClassroomVolume.Text),
                ClassroomType = (ClassroomType)ComboBoxClassroomClassroomType.SelectedItem
            };
            db.Classroom.Add(room);
            db.SaveChanges();
            LoadData();
        }
    }
}
